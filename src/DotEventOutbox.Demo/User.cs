using System.ComponentModel.DataAnnotations.Schema;
using DotEventOutbox.Contracts;
using MediatR;

namespace DotEventOutbox.Demo;

public class User : IDomainEventEmitter
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    private readonly List<DomainEvent> _events = [];
    [NotMapped]
    public IReadOnlyCollection<DomainEvent> Events => _events.AsReadOnly();

    public User(Guid id, string name, string email)
    {
        Id = id;
        Name = name;
        Email = email;

        _events.Add(new UserCreatedDomainEvent(name, email));
    }

    public void ClearEvents()
    {
        _events.Clear();
    }
}

public record UserCreatedDomainEvent(string Name, string Email) : DomainEvent;

public class UserCreatedSendEmailHandler : INotificationHandler<UserCreatedDomainEvent>
{
    private readonly Random _random = new();
    public Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Trying to send email to {notification.Email}...");

        if (_random.Next(2) == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Failed to send email to {notification.Email}.", ConsoleColor.Red);
            Console.ResetColor();
            throw new Exception("Failed to send email");
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Email sent to {notification.Email}.", ConsoleColor.Green);
        Console.ResetColor();
        return Task.CompletedTask;
    }
}
