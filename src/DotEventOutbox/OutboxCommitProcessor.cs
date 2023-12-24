using System.Data;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using DotEventOutbox.Contracts;
using DotEventOutbox.Persistence;
using DotEventOutbox.Entities;

namespace DotEventOutbox;

/// <summary>
/// Internal service for processing and saving domain events as outbox messages in the database.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="OutboxCommitProcessor"/> class.
/// </remarks>
/// <param name="outboxDbContext">Database context for the outbox.</param>
/// <exception cref="ArgumentNullException">Thrown if <paramref name="outboxDbContext"/> is null.</exception>
internal sealed class OutboxCommitProcessor(OutboxDbContext outboxDbContext) : IOutboxCommitProcessor
{
    private readonly OutboxDbContext outboxDbContext = outboxDbContext ?? throw new ArgumentNullException(nameof(outboxDbContext));

    /// <summary>
    /// Processes and saves domain events from the specified <paramref name="dbContext"/> as outbox messages.
    /// </summary>
    /// <param name="dbContext">The DbContext to extract and process domain events from.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ProcessAndSaveAsync(DbContext dbContext, CancellationToken cancellationToken = default)
    {
        ConvertDomainEventsToOutboxMessages(dbContext);

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await dbContext.SaveChangesAsync(cancellationToken);
        await outboxDbContext.SaveChangesAsync(cancellationToken);
        transaction.Complete();
    }

    /// <summary>
    /// Converts domain events extracted from the specified <paramref name="dbContext"/> into outbox messages,
    /// readying them for subsequent processing and delivery. This conversion includes serialization of the event data.
    /// </summary>
    /// <param name="dbContext">The DbContext from which to extract domain events for conversion.</param>
    private void ConvertDomainEventsToOutboxMessages(DbContext dbContext)
    {
        var entitiesWithEvents = dbContext.ChangeTracker.Entries<IDomainEventEmitter>()
            .Select(e => e.Entity);

        var outboxMessages = entitiesWithEvents
            .SelectMany(entity =>
            {
                var domainEvents = entity.Events.ToList();
                entity.ClearEvents();
                return domainEvents;
            })
            .Select(domainEvent => new OutboxMessage
            {
                Id = domainEvent.Id,
                EventType = domainEvent.GetType().Name,
                Content = JsonConvert.SerializeObject(domainEvent, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }),
                OccurredOnUtc = domainEvent.OccurredOnUtc,
            })
            .ToList();

        outboxDbContext.Set<OutboxMessage>().AddRange(outboxMessages);
    }
}