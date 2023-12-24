namespace DotEventOutbox.Entities;

/// <summary>
/// Represents a dead letter message within the outbox pattern framework.
/// This entity is crucial for storing details about messages that have failed processing,
/// enabling later analysis, troubleshooting, and potential reprocessing.
/// </summary>
public class DeadLetterMessage
{
    /// <summary>
    /// Unique identifier for the dead letter message. This GUID facilitates individual tracking and management of failed messages.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Type of the dead letter message, often reflecting the original event or message type.
    /// This helps categorize the failure in the context of the message's intended purpose.
    /// </summary>
    public string EventType { get; set; } = string.Empty;


    /// <summary>
    /// Serialized content of the message. This may include the original event data, providing context for the failure.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp in UTC indicating when the original message was created or the corresponding event occurred.
    /// This timestamp is essential for understanding the timeline of the message's lifecycle.
    /// </summary>
    public DateTime OccurredOnUtc { get; set; }

    /// <summary>
    /// Error message or description detailing the cause of the processing failure.
    /// This information is vital for diagnosing issues and planning reprocessing strategies.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Number of retry attempts made to process the message. This count is useful for tracking retry efforts and determining processing strategies.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Timestamp in UTC of the last encountered error during processing.
    /// This helps in identifying the most recent issue and assessing the message's processing history.
    /// </summary>
    public DateTime LastErrorOccurredOnUtc { get; set; }
}
