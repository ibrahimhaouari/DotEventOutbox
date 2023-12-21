using MediatR;

namespace DotEventOutbox.Contracts;

/// <summary>
/// Represents a domain event. All events in the domain should implement this interface.
/// It extends from MediatR.INotification, enabling it to be used with MediatR handlers.
/// </summary>
public interface IEvent : INotification
{
    /// <summary>
    /// Gets the unique identifier for the event.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the date and time in UTC when the event occurred.
    /// </summary>
    DateTime OccurredOnUtc { get; }
}
