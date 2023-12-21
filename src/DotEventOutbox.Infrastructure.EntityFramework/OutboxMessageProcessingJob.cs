using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Quartz;
using DotEventOutbox.Entities;
using DotEventOutbox.Contracts;
using DotEventOutbox.Infrastructure.Common.Configuration;

namespace DotEventOutbox.Infrastructure.EntityFramework;

// Prevent multiple instances of the job from running concurrently
[DisallowConcurrentExecution]
public class OutboxMessageProcessingJob(
    OutboxDbContext dbContext,
    IPublisher publisher,
    IOptions<EventOutboxConfiguration> options,
    ILogger<OutboxMessageProcessingJob> logger) : IJob
{
    private readonly OutboxDbContext dbContext = dbContext;
    private readonly IPublisher publisher = publisher;
    private readonly EventOutboxConfiguration configuration = options.Value;
    private readonly ILogger<OutboxMessageProcessingJob> logger = logger;

    public async Task Execute(IJobExecutionContext context)
    {
        // Fetch pending outbox messages ordered by their occurrence time
        var messages = await dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(configuration.MaxMessagesProcessedPerBatch)
            .ToListAsync(context.CancellationToken);

        foreach (var message in messages)
        {
            try
            {
                logger.LogInformation("Processing outbox message with ID {Id}.", message.Id);

                // Deserialize the message content into an event
                var @event = JsonConvert.DeserializeObject<DomainEvent>(message.Content, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                }) ?? throw new InvalidOperationException("Deserialized event is null.");

                // Create a retry policy and attempt to publish the event
                var retryPolicy = Policy.Handle<Exception>()
                    .WaitAndRetryAsync(configuration.MaxRetryAttempts, retryAttempt => TimeSpan.FromMilliseconds(retryAttempt * configuration.RetryIntervalInMilliseconds));

                var result = await retryPolicy.ExecuteAndCaptureAsync(async () =>
                 await publisher.Publish(@event, context.CancellationToken));

                if (result.Outcome == OutcomeType.Failure)
                {
                    throw result.FinalException;
                }

                logger.LogInformation("Outbox message with ID {Id} processed successfully.", message.Id);

                // Mark the message as processed
                message.ProcessedOnUtc = DateTime.UtcNow;
            }
            catch (Exception e)
            {
                // Log error (to be implemented)
                logger.LogError(e, "Failed to process outbox message with ID {Id}.", message.Id);

                // Move the failed message to the dead letter queue
                var deadLetterMessage = new DeadLetterMessage
                {
                    Id = message.Id,
                    Type = message.Type,
                    Content = message.Content,
                    OccurredOnUtc = message.OccurredOnUtc,
                    Error = e.ToString(),
                    Retries = configuration.MaxRetryAttempts,
                    LastErrorOccurredOnUtc = DateTime.UtcNow,
                };

                dbContext.Set<DeadLetterMessage>().Add(deadLetterMessage);
                dbContext.Set<OutboxMessage>().Remove(message);
            }
        }

        // Save changes to the database
        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
