using System.Data;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using DotEventOutbox.Contracts;
using DotEventOutbox.Persistence;
using DotEventOutbox.Entities;

namespace DotEventOutbox;

/// <summary>
/// Service for converting domain events to outbox messages and persisting them in a database.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="OutboxCommitProcessor"/> class.
/// </remarks>
/// <param name="outboxDbContext">The database context for the outbox.</param>
public class OutboxCommitProcessor(OutboxDbContext outboxDbContext)
{
    private readonly OutboxDbContext outboxDbContext = outboxDbContext ?? throw new ArgumentNullException(nameof(outboxDbContext));

    /// <summary>
    /// Processes domain events from the specified <paramref name="dbContext"/>, converts them to outbox messages,
    /// and saves them to the database.
    /// </summary>
    /// <param name="dbContext">The DbContext from which to extract and process domain events.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async Task ProcessAndSaveAsync(DbContext dbContext, CancellationToken cancellationToken = default)
    {
        ConvertDomainEventsToOutboxMessages(dbContext);

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await dbContext.SaveChangesAsync(cancellationToken);
        await outboxDbContext.SaveChangesAsync(cancellationToken);
        transaction.Complete();
    }

    /// <summary>
    /// Converts domain events from the specified <paramref name="dbContext"/> to outbox messages.
    /// </summary>
    /// <param name="dbContext">The DbContext from which to extract domain events.</param>
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
                Type = domainEvent.GetType().Name,
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
