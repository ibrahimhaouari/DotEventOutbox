namespace DotEventOutbox.Common.Entities;

/// <summary>
/// Represents a dead letter message in the outbox pattern.
/// This entity is used to store information about messages that failed to process,
/// allowing for later review and potential reprocessing.
/// </summary>
public class DeadLetterMessage
{
    /// <summary>
    /// Gets or sets the unique identifier of the dead letter message.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the type of the dead letter message, typically representing the event or message type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized content of the dead letter message.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time in UTC when the message was initially created or the event occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; set; }

    /// <summary>
    /// Gets or sets the error message or description associated with the processing failure of the message.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Gets or sets the number of attempts that have been made to process the message.
    /// </summary>
    public int Retries { get; set; }

    /// <summary>
    /// Gets or sets the date and time in UTC of the last error occurrence during message processing.
    /// </summary>
    public DateTime LastErrorOccurredOnUtc { get; set; }
}
