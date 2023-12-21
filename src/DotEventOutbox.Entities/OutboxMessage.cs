namespace DotEventOutbox.Entities;

/// <summary>
/// Represents a message within the outbox.
/// This class is used to store information about domain events that are to be
/// published to external systems or processed asynchronously.
/// </summary>
public class OutboxMessage
{
    /// <summary>
    /// Gets or sets the unique identifier of the outbox message.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the type of the outbox message, typically representing the event type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized content of the outbox message. This could be a serialized domain event.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time in UTC when the outbox message was created or the event occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the date and time in UTC when the outbox message was processed. Null if the message hasn't been processed yet.
    /// </summary>
    public DateTime? ProcessedOnUtc { get; set; }
}
