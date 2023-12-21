namespace DotEventOutbox.Contracts;

/// <summary>
/// Defines the contract for an emitter of domain events.
/// Implementors of this interface are capable of tracking and managing domain events.
/// </summary>
public interface IDomainEventEmitter
{
    /// <summary>
    /// Gets a read-only collection of domain events that have been emitted.
    /// </summary>
    IReadOnlyCollection<DomainEvent> Events { get; }

    /// <summary>
    /// Clears all tracked domain events from the emitter.
    /// This is typically called after the events have been successfully processed.
    /// </summary>
    void ClearEvents();
}
