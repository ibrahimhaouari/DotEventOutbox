namespace DotEventOutbox.Infrastructure.Common.Configuration;

/// <summary>
/// Represents the configuration settings for the DotEventOutbox.
/// </summary>
public class EventOutboxConfiguration
{
    /// <summary>
    /// The configuration section name in the application configuration file.
    /// </summary>
    public const string SectionName = "DotEventOutbox";

    /// <summary>
    /// Gets or sets the interval in seconds between each execution of the outbox processing job.
    /// Default value is 10 seconds.
    /// </summary>
    public int ProcessingIntervalInSeconds { get; set; } = 10;

    /// <summary>
    /// Gets or sets the maximum number of outbox messages that will be processed in each batch.
    /// Default value is 10 messages.
    /// </summary>
    public int MaxMessagesProcessedPerBatch { get; set; } = 10;

    /// <summary>
    /// Gets or sets the interval in milliseconds between each retry attempt when processing an outbox message fails.
    /// Default value is 50 milliseconds.
    /// </summary>
    public int RetryIntervalInMilliseconds { get; set; } = 50;

    /// <summary>
    /// Gets or sets the maximum number of attempts to retry processing an outbox message before it is moved to the dead-letter queue.
    /// Default value is 3 attempts.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
}
