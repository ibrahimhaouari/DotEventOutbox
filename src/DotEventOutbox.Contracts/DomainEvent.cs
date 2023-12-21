namespace DotEventOutbox.Contracts;

public abstract record DomainEvent : IEvent
{
    public Guid Id { get; }
    public DateTime OccurredOnUtc { get; }

    protected DomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredOnUtc = DateTime.UtcNow;
    }
}