namespace DotEventOutbox.Contracts;

/// <summary>
/// An abstract base record for domain events.
/// It provides a default implementation for the event ID and the timestamp
/// when the event occurred, ensuring all domain events have a unique identifier
/// and a consistent time reference.
/// </summary>
public abstract record DomainEvent : IEvent
{
    /// <summary>
    /// Gets the unique identifier for the event, automatically generated upon instantiation.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the date and time in UTC when the event occurred, set to the current UTC time upon instantiation.
    /// </summary>
    public DateTime OccurredOnUtc { get; init; }

    protected DomainEvent(Guid id, DateTime occurredOnUtc)
    {
        Id = id;
        OccurredOnUtc = occurredOnUtc;
    }

    protected DomainEvent()
        : this(Guid.NewGuid(), DateTime.UtcNow)
    {
    }
}