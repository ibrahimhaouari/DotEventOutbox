namespace DotEventOutbox.Entities;


public class DeadLetterMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime OccurredOnUtc { get; set; }
    public string? Error { get; set; }
    public int Retries { get; set; }
    public DateTime LastErrorOccurredOnUtc { get; set; }
}
