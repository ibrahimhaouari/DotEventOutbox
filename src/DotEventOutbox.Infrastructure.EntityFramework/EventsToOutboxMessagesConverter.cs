using System.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using DotEventOutbox.Contracts;
using DotEventOutbox.Entities;

namespace DotEventOutbox.Infrastructure.EntityFramework;

/// <summary>
/// Service for converting domain events to outbox messages.
/// </summary>
public class EventsToOutboxMessagesConverter
{
    private readonly OutboxDbContext _outboxDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventsToOutboxMessagesConverter"/> class.
    /// </summary>
    /// <param name="outboxDbContext">The outbox database context.</param>
    public EventsToOutboxMessagesConverter(OutboxDbContext outboxDbContext)
    {
        _outboxDbContext = outboxDbContext;
    }

    /// <summary>
    /// Converts the domain events of entities being tracked by the given application database context to outbox messages.
    /// </summary>
    /// <param name="dbContext">The application database context tracking the entities.</param>
    public void Convert(DbContext dbContext)
    {
        // Get the entities with domain events from the change tracker.
        var entitiesWithEvents = dbContext.ChangeTracker.Entries<IDomainEventEmitter>()
           .Select(e => e.Entity);

        // Convert each domain event to an outbox message.
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

        // Add the outbox messages to the outbox database context.
        _outboxDbContext.Set<OutboxMessage>().AddRange(outboxMessages);
    }
}
