using Microsoft.EntityFrameworkCore;
using DotEventOutbox.Persistence;
using DotEventOutbox.Entities;
using DotEventOutbox.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotEventOutbox.Tests;

public class OutboxCommitProcessorTests
{
    private static OutboxDbContext GetOutboxDbContext() => new(new DbContextOptionsBuilder<OutboxDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options);

    private static MockDbContext GetDbContext() => new(new DbContextOptionsBuilder<MockDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options);

    private class MockEventEmitter : IDomainEventEmitter
    {
        public Guid Id { get; set; }
        private readonly List<DomainEvent> _events = [];
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

    private record MockDomainEvent() : DomainEvent;

    private class MockDbContext(DbContextOptions<MockDbContext> options) : DbContext(options)
    {
        public DbSet<MockEventEmitter> MockEventEmitters { get; set; } = default!;
    }

    [Fact]
    public async Task ProcessAndSaveAsync_SavesOutboxMessages_WhenDomainEventsAreRaised()
    {
        // Arrange
        var outboxDbContext = GetOutboxDbContext();
        var dbContext = GetDbContext();
        var processor = new OutboxCommitProcessor(outboxDbContext);

        var domainEvent = new MockDomainEvent();
        var eventEmitter = new MockEventEmitter();
        eventEmitter.RaiseEvent(domainEvent);
        dbContext.MockEventEmitters.Add(eventEmitter);

        // Act
        await processor.ProcessAndSaveAsync(dbContext);

        // Assert
        var outboxMessages = outboxDbContext.Set<OutboxMessage>().ToList();
        Assert.Single(outboxMessages);
        Assert.Equal(nameof(MockDomainEvent), outboxMessages.First().EventType);
        Assert.Equal(domainEvent.Id, outboxMessages.First().Id);
        Assert.Equal(domainEvent.OccurredOnUtc, outboxMessages.First().OccurredOnUtc);
        Assert.Null(outboxMessages.First().ProcessedOnUtc);
        var savedEvent = DomainEventJsonConverter.Deserialize<MockDomainEvent>(outboxMessages.First().Content);
        Assert.Equal(savedEvent, domainEvent);
    }

    [Fact]
    public async Task ProcessAndSaveAsync_SavesOutboxMessages_WhenMultipleDomainEventsAreRaised()
    {
        // Arrange
        var outboxDbContext = GetOutboxDbContext();
        var dbContext = GetDbContext();
        var processor = new OutboxCommitProcessor(outboxDbContext);

        var domainEvent1 = new MockDomainEvent();
        var domainEvent2 = new MockDomainEvent();
        var eventEmitter = new MockEventEmitter();
        eventEmitter.RaiseEvent(domainEvent1);
        eventEmitter.RaiseEvent(domainEvent2);
        dbContext.MockEventEmitters.Add(eventEmitter);

        // Act
        await processor.ProcessAndSaveAsync(dbContext);

        // Assert
        var outboxMessages = outboxDbContext.Set<OutboxMessage>().ToList();
        Assert.Equal(2, outboxMessages.Count);
        Assert.Equal(domainEvent1.Id, outboxMessages.First().Id);
        Assert.Equal(domainEvent2.Id, outboxMessages.Last().Id);
    }

    [Fact]
    public async Task ProcessAndSaveAsync_SaveDbContextChanges()
    {
        // Arrange
        var outboxDbContext = GetOutboxDbContext();
        var dbContext = GetDbContext();
        var processor = new OutboxCommitProcessor(outboxDbContext);

        var domainEvent = new MockDomainEvent();
        var eventEmitter = new MockEventEmitter();
        eventEmitter.RaiseEvent(domainEvent);
        dbContext.MockEventEmitters.Add(eventEmitter);

        // Act
        await processor.ProcessAndSaveAsync(dbContext);

        // Assert
        var mockEventEmitters = dbContext.MockEventEmitters.ToList();
        Assert.Single(mockEventEmitters);
        Assert.Equal(eventEmitter.Id, mockEventEmitters.First().Id);
    }

    [Fact]
    public async Task ProcessAndSaveAsync_SaveOutboxMessagesOnce_WhenProcessingSameEventMultipleTimes()
    {
        // Arrange
        var outboxDbContext = GetOutboxDbContext();
        var dbContext = GetDbContext();
        var processor = new OutboxCommitProcessor(outboxDbContext);

        var domainEvent = new MockDomainEvent();
        var eventEmitter = new MockEventEmitter();
        eventEmitter.RaiseEvent(domainEvent);
        dbContext.MockEventEmitters.Add(eventEmitter);

        // Act
        await processor.ProcessAndSaveAsync(dbContext);
        await processor.ProcessAndSaveAsync(dbContext);

        // Assert
        var outboxMessages = outboxDbContext.Set<OutboxMessage>().ToList();
        Assert.Single(outboxMessages);
    }

    [Fact]
    public async Task ProcessAndSaveAsync_DoesNotCreateOutboxMessages_WhenNoDomainEventsAreRaised()
    {
        // Arrange
        var outboxDbContext = GetOutboxDbContext();
        var dbContext = GetDbContext();
        var processor = new OutboxCommitProcessor(outboxDbContext);

        var eventEmitter = new MockEventEmitter();
        dbContext.MockEventEmitters.Add(eventEmitter);

        // Act
        await processor.ProcessAndSaveAsync(dbContext);

        // Assert
        var outboxMessages = outboxDbContext.Set<OutboxMessage>().ToList();
        Assert.Empty(outboxMessages);
    }
}
