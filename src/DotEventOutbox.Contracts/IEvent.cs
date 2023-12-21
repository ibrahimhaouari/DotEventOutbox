using MediatR;

namespace DotEventOutbox.Contracts;

public interface IEvent : INotification
{
    Guid Id { get; }
    DateTime OccurredOnUtc { get; }
}
