namespace DotEventOutbox.Entities;

/// <summary>
/// Represents a consumer of outbox messages. This entity is essential for identifying
/// different systems or processes that consume messages from the outbox. It facilitates
/// the tracking and management of message consumption across various consumers.
/// </summary>
public sealed class OutboxMessageConsumer
{
    /// <summary>
    /// Unique identifier for the outbox message consumer. This GUID is crucial for uniquely identifying
    /// and differentiating between multiple consumers in the system.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name representing the identity of the consumer. This could be the name of a system, service, or
    /// any logical entity that processes the outbox messages. Naming helps in easier identification and
    /// management of consumers.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}