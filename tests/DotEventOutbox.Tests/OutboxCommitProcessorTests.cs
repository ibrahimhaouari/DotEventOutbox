using Microsoft.EntityFrameworkCore;
using DotEventOutbox.Persistence;
using DotEventOutbox.Entities;
using DotEventOutbox.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotEventOutbox.Tests;

public class OutboxCommitProcessorTests
{
    // Set up an in-memory database for OutboxDbContext
    private static OutboxDbContext GetOutboxDbContext() => new(new DbContextOptionsBuilder<OutboxDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options);

    // Mock entity that implements IDomainEventEmitter
    private class MockEventEmitter : IDomainEventEmitter
    {
        public Guid Id { get; set; }
        private readonly List<DomainEvent> _events = new List<DomainEvent>();
        [NotMapped]
        public IReadOnlyCollection<DomainEvent> Events => _events.AsReadOnly();

        public void RaiseEvent(DomainEvent domainEvent)
        {
            _events.Add(domainEvent);
        }

        public void ClearEvents()
        {
            _events.Clear();
        }
    }

    // Mock domain event for testing
    private record MockDomainEvent() : DomainEvent;

    // Mock DbContext that includes the MockEventEmitter entity
    private class MockDbContext(DbContextOptions<MockDbContext> options) : DbContext(options)
    {
        public DbSet<MockEventEmitter> MockEventEmitters { get; set; } = default!;
    }

    // Set up an in-memory database for MockDbContext
    private static MockDbContext GetDbContext() => new(new DbContextOptionsBuilder<MockDbContext>()
        .UseInMemoryDatabase(databaseName: "TestDb").Options);

    [Fact]
    public async Task ProcessAndSaveAsync_SavesOutboxMessages_WhenDomainEventsAreRaised()
    {
        // Arrange
        var outboxDbContext = GetOutboxDbContext();
        var dbContext = GetDbContext();
        var processor = new OutboxCommitProcessor(outboxDbContext);
        var eventEmitter = new MockEventEmitter();
        var domainEvent = new MockDomainEvent();
        eventEmitter.RaiseEvent(domainEvent);
        dbContext.MockEventEmitters.Add(eventEmitter);

        // Act
        await processor.ProcessAndSaveAsync(dbContext);

        // Assert
        var outboxMessages = outboxDbContext.Set<OutboxMessage>().ToList();
        Assert.Single(outboxMessages); // Check if one message is saved
        Assert.Equal(nameof(MockDomainEvent), outboxMessages.First().EventType); // Check the type of the event
        Assert.Equal(domainEvent.Id, outboxMessages.First().Id); // Check the event id
    }

    [Fact]
    public async Task ProcessAndSaveAsync_IsIdempotent_WhenProcessingSameEventMultipleTimes()
    {
        // Arrange: Set up a processor and raise an event
        var outboxDbContext = GetOutboxDbContext();
        var dbContext = GetDbContext();
        var processor = new OutboxCommitProcessor(outboxDbContext);
        var eventEmitter = new MockEventEmitter();
        var domainEvent = new MockDomainEvent();
        eventEmitter.RaiseEvent(domainEvent);
        dbContext.MockEventEmitters.Add(eventEmitter);

        // Act: Process the same event twice
        await processor.ProcessAndSaveAsync(dbContext);
        await processor.ProcessAndSaveAsync(dbContext); // Processing again

        // Assert: Only one outbox message should be created
        var outboxMessages = outboxDbContext.Set<OutboxMessage>().ToList();
        Assert.Single(outboxMessages);
    }

    [Fact]
    public async Task ProcessAndSaveAsync_DoesNotCreateOutboxMessages_WhenNoDomainEventsAreRaised()
    {
        // Arrange: Create a processor with an empty DbContext
        var outboxDbContext = GetOutboxDbContext();
        var dbContext = GetDbContext();
        var processor = new OutboxCommitProcessor(outboxDbContext);

        // Act: Process with an empty DbContext
        await processor.ProcessAndSaveAsync(dbContext);

        // Assert: No outbox messages should be created
        var outboxMessages = outboxDbContext.Set<OutboxMessage>().ToList();
        Assert.Empty(outboxMessages);
    }


}
