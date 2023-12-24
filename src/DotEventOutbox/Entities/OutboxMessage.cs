namespace DotEventOutbox.Entities;

/// <summary>
/// Represents an outbox message. This class is pivotal in storing information about domain events
/// that need to be published to external systems or processed asynchronously. It encapsulates all necessary
/// details of a domain event for delayed processing and ensures reliable delivery.
/// </summary>
public class OutboxMessage
{
    /// <summary>
    /// Unique identifier of the outbox message. This GUID ensures each message can be individually tracked and processed.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Type of the outbox message, generally reflecting the associated domain event type.
    /// This categorization aids in identifying and handling the message appropriately.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Serialized representation of the outbox message, often a domain event.
    /// This serialization allows for flexible, format-agnostic storage of event data.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp in UTC indicating when the message was created or the corresponding event occurred.
    /// This is crucial for time-sensitive processing and auditing.
    /// </summary>
    public DateTime OccurredOnUtc { get; set; }

    /// <summary>
    /// Timestamp in UTC marking when the message was processed. A null value indicates that the message is pending processing.
    /// This helps in tracking the processing status of messages in the outbox.
    /// </summary>
    public DateTime? ProcessedOnUtc { get; set; }
}