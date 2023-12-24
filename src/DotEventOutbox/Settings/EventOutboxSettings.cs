namespace DotEventOutbox.Settings;

/// <summary>
/// Represents the configurable settings for the DotEventOutbox.
/// These settings allow for customization of the outbox processing behavior,
/// including intervals, batch sizes, and retry mechanisms.
/// </summary>
public class EventOutboxSettings
{
    /// <summary>
    /// The name of the configuration section in the application configuration file dedicated to DotEventOutbox.
    /// This constant helps in standardizing and locating the outbox settings within the application configuration.
    /// </summary>
    public const string SectionName = "DotEventOutbox";

    /// <summary>
    /// Interval in seconds between each execution of the outbox processing job.
    /// The default value is set to 10 seconds, balancing responsiveness and resource utilization.
    /// </summary>
    public int ProcessingIntervalInSeconds { get; set; } = 10;

    /// <summary>
    /// Maximum number of outbox messages to be processed in each batch.
    /// Default is 10 messages, optimizing batch processing efficiency without overwhelming system resources.
    /// </summary>
    public int MaxMessagesProcessedPerBatch { get; set; } = 10;

    /// <summary>
    /// Interval in milliseconds between retry attempts when processing an outbox message fails.
    /// Set to a default of 50 milliseconds, providing a quick retry mechanism to handle transient issues.
    /// </summary>
    public int RetryIntervalInMilliseconds { get; set; } = 50;

    /// <summary>
    /// Maximum number of attempts to retry processing an outbox message before it is moved to the dead-letter queue.
    /// With a default of 3 attempts, this setting strikes a balance between retrying for recoverable errors and avoiding infinite retries.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
}