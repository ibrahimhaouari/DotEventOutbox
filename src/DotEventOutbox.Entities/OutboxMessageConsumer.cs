namespace DotEventOutbox.Entities;

public sealed class OutboxMessagesConsumer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}
