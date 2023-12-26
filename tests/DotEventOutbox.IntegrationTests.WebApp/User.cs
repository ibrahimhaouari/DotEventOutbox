using System.ComponentModel.DataAnnotations.Schema;
using DotEventOutbox.Contracts;
using MediatR;

namespace DotEventOutbox.IntegrationTests.WebApp;

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

    public void Raise(DomainEvent domainEvent)
    {
        _events.Add(domainEvent);
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
public class UserCreatedSendEmailHandler : INotificationHandler<UserCreatedDomainEvent>
{
    private readonly Random _random = new();
    public Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Email sent to {notification.Email}.", ConsoleColor.Green);
        Console.ResetColor();
        return Task.CompletedTask;
    }
}

public class UserCreatedSendEmailFailedHandler : INotificationHandler<UserCreatedDomainEvent>
{
    private readonly Random _random = new();
    public Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Failed to send email to {notification.Email}.", ConsoleColor.Red);
        Console.ResetColor();
        throw new Exception("Failed to send email");
    }
}
