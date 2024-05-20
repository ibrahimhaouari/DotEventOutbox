using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Quartz;
using DotEventOutbox.Contracts;
using DotEventOutbox.Persistence;
using DotEventOutbox.Settings;
using DotEventOutbox.Entities;
using System.Diagnostics;

namespace DotEventOutbox;


/// <summary>
/// A Quartz job designed for processing messages stored in the outbox.
/// It's responsible for retrieving, deserializing, and publishing domain events,
/// and handles moving failed messages to a dead letter queue for further analysis or reprocessing.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="OutboxMessageProcessingJob"/> class with necessary dependencies.
/// </remarks>
/// <param name="dbContext">The database context for the outbox, used for retrieving and updating messages.</param>
/// <param name="publisher">The publisher for dispatching domain events.</param>
/// <param name="options">Configuration options for the outbox.</param>
/// <param name="logger">Logger for recording job execution details.</param>
/// <exception cref="ArgumentNullException">Thrown if any argument is null.</exception>
[DisallowConcurrentExecution]
internal sealed class OutboxMessageProcessingJob(
    OutboxDbContext dbContext,
    IPublisher publisher,
    IOptions<EventOutboxSettings> options,
    ILogger<OutboxMessageProcessingJob> logger) : IJob
{
    private readonly OutboxDbContext dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly IPublisher publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    private readonly EventOutboxSettings configuration = options?.Value ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<OutboxMessageProcessingJob> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Executes the job to process outbox messages. It involves deserializing and publishing each message,
    /// applying retry policies, and handling failures by moving messages to the dead letter queue.
    /// </summary>
    /// <param name="context">The execution context for the job, providing runtime information.</param>
    /// <returns>A task representing the asynchronous job execution.</returns>
    public async Task Execute(IJobExecutionContext context)
    {
        var messages = await dbContext.Set<OutboxMessage>()
            .TagWith("GetPendingOutboxMessages")
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(configuration.MaxMessagesProcessedPerBatch)
            .ToListAsync(context.CancellationToken);

        // Add messages count tag to the current activity for distributed tracing
        Activity.Current?.AddTag("messages.count", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                logger.LogInformation("Processing outbox message with ID {Id}.", message.Id);

                var @event = DomainEventJsonConverter.Deserialize<DomainEvent>(message.Content)
                ?? throw new InvalidOperationException("Deserialized event is null.");

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
                    EventType = message.EventType,
                    Content = message.Content,
                    OccurredOnUtc = message.OccurredOnUtc,
                    Error = e.ToString(),
                    RetryCount = configuration.MaxRetryAttempts,
                    LastErrorOccurredOnUtc = DateTime.UtcNow,
                };

                dbContext.Set<DeadLetterMessage>().Add(deadLetterMessage);
                dbContext.Set<OutboxMessage>().Remove(message);
            }
        }

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
