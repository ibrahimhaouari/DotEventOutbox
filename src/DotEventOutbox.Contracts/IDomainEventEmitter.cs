namespace DotEventOutbox.Contracts;

public interface IDomainEventEmitter
{
    IReadOnlyCollection<DomainEvent> Events { get; }
    void ClearEvents();
}

