using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using DotEventOutbox.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DotEventOutbox.Demo;

/// <summary>
/// Represents a user entity capable of emitting domain events.
/// </summary>
public class User : IDomainEventEmitter
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    private readonly List<DomainEvent> _events = [];
    [NotMapped]
    public IReadOnlyCollection<DomainEvent> Events => _events.AsReadOnly();

    /// <summary>
    /// Creates a new user and raises a UserCreatedDomainEvent.
    /// </summary>
    public User(Guid id, string name, string email)
    {
        Id = id;
        Name = name;
        Email = email;
    }

    public void RaiseEvent(DomainEvent @event)
    {
        _events.Add(@event);
    }

    public void ClearEvents()
    {
        _events.Clear();
    }
}

/// <summary>
/// Domain event that is raised when a new user is created.
/// </summary>
public record UserCreatedDomainEvent(string Name, string Email) : DomainEvent;

/// <summary>
/// Handles the UserCreatedDomainEvent by simulating an email send operation.
/// </summary>
public class UserCreatedSendEmailHandler(ILogger<UserCreatedSendEmailHandler> logger)
 : INotificationHandler<UserCreatedDomainEvent>
{
    private readonly ILogger<UserCreatedSendEmailHandler> logger = logger;
    private readonly Random _random = new();
    public Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Trying to send email to {Email}...", notification.Email);

        // Randomly simulate success or failure in sending email
        if (_random.Next(2) == 0)
        {
            logger.LogError("Failed to send email to {Email}.", notification.Email);
            throw new Exception("Failed to send email");
        }

        logger.LogInformation("Email sent to {Email}.", notification.Email);
        return Task.CompletedTask;
    }
}
