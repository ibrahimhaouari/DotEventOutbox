namespace DotEventOutbox.Entities;

/// <summary>
/// Represents a consumer of outbox messages. This entity typically identifies
/// a system or process that consumes messages stored in the outbox.
/// </summary>
public sealed class OutboxMessagesConsumer
{
    /// <summary>
    /// Gets or sets the unique identifier of the outbox messages consumer.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the outbox messages consumer.
    /// This name usually represents the identity of the system or service consuming the messages.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
