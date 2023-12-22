using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Quartz;
using DotEventOutbox.Contracts;
using DotEventOutbox.Persistence;
using DotEventOutbox.Settings;
using DotEventOutbox.Entities;

namespace DotEventOutbox;

/// <summary>
/// Quartz job for processing messages stored in the outbox.
/// This job handles the retrieval, deserialization, and publishing of domain events,
/// as well as moving failed messages to a dead letter queue.
/// </summary>
[DisallowConcurrentExecution]
internal class OutboxMessageProcessingJob(
    OutboxDbContext dbContext,
    IPublisher publisher,
    IOptions<EventOutboxSettings> options,
    ILogger<OutboxMessageProcessingJob> logger) : IJob
{
    private readonly OutboxDbContext dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly IPublisher publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    private readonly EventOutboxSettings configuration = options?.Value ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<OutboxMessageProcessingJob> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task Execute(IJobExecutionContext context)
    {
        var messages = await dbContext.Set<OutboxMessage>()
            .TagWith("GetPendingOutboxMessages")
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(configuration.MaxMessagesProcessedPerBatch)
            .ToListAsync(context.CancellationToken);

        foreach (var message in messages)
        {
            try
            {
                logger.LogInformation("Processing outbox message with ID {Id}.", message.Id);

                var @event = JsonConvert.DeserializeObject<IEvent>(message.Content, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                }) ?? throw new InvalidOperationException("Deserialized event is null.");

                var retryPolicy = Policy.Handle<Exception>()
                    .WaitAndRetryAsync(configuration.MaxRetryAttempts, retryAttempt =>
                        TimeSpan.FromMilliseconds(retryAttempt * configuration.RetryIntervalInMilliseconds));

                var result = await retryPolicy.ExecuteAndCaptureAsync(async () =>
                    await publisher.Publish(@event, context.CancellationToken));

                if (result.Outcome == OutcomeType.Failure)
                {
                    throw result.FinalException;
                }

                logger.LogInformation("Outbox message with ID {Id} processed successfully.", message.Id);

                message.ProcessedOnUtc = DateTime.UtcNow;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to process outbox message with ID {Id}.", message.Id);

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

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
