namespace DotEventOutbox.Contracts;

public abstract record DomainEvent : IEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }

    protected DomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}