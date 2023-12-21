namespace DotEventOutbox.Infrastructure.Common.Configuration;

public class EventOutboxConfiguration
{
    public const string SectionName = "DotEventOutbox";
    public int ProcessingIntervalInSeconds { get; set; } = 10;
    public int MaxMessagesProcessedPerBatch { get; set; } = 10;
    public int RetryIntervalInMilliseconds { get; set; } = 50;
    public int MaxRetryAttempts { get; set; } = 3;
}
