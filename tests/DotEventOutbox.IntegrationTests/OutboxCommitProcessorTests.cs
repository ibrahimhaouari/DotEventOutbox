using DotEventOutbox.Entities;
using DotEventOutbox.IntegrationTests.WebApp;
using DotEventOutbox.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DotEventOutbox.IntegrationTests;

public class OutboxCommitProcessorTests(IntegrationTestsWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task ProcessAndSaveAsync_ShouldSaveChangesToDatabase()
    {
        // Arrange
        var dbContext = GetService<AppDbContext>();
        var user = new User(Guid.NewGuid(), "John Doe", "John.Doe@Test.com");
        dbContext.Users.Add(user);
        var outboxCommitProcessor = GetService<IOutboxCommitProcessor>();

        // Act
        await outboxCommitProcessor.ProcessAndSaveAsync(dbContext);

        // Assert
        var savedUser = await dbContext.Users.FindAsync(user.Id);
        Assert.NotNull(savedUser);
        Assert.Equal(user.Id, savedUser.Id);
    }

    [Fact]
    public async Task ProcessAndSaveAsync_ShouldSaveDomainEventToDatabase()
    {
        // Arrange
        var dbContext = GetService<AppDbContext>();
        var user = new User(Guid.NewGuid(), "John Doe", "John.Doe@Test.com");
        var domainEvent = new UserCreatedDomainEvent(user.Name, user.Email);
        user.Raise(domainEvent);
        dbContext.Users.Add(user);
        var outboxCommitProcessor = GetService<IOutboxCommitProcessor>();
        var outboxDbContext = GetService<OutboxDbContext>();

        // Act
        await outboxCommitProcessor.ProcessAndSaveAsync(dbContext);

        // Assert
        var outboxMessage = await outboxDbContext.Set<OutboxMessage>().FindAsync(domainEvent.Id);
        Assert.NotNull(outboxMessage);
        Assert.Equal(domainEvent.OccurredOnUtc, outboxMessage.OccurredOnUtc);
        Assert.Equal(domainEvent, DomainEventJsonConverter.Deserialize<UserCreatedDomainEvent>(outboxMessage.Content));
    }

    [Fact]
    public async Task ProcessAndSaveAsync_ShouldRollbackIfSaveChangesFails()
    {
        // Arrange
        var dbContext = GetService<AppDbContext>();
        var user = new User(Guid.NewGuid(), "John Doe", "John.Doe@Test.com");
        dbContext.Users.Add(user);
        dbContext.SaveChanges();
        dbContext.ChangeTracker.Clear();

        var domainEvent = new UserCreatedDomainEvent(user.Name, user.Email);
        user.Raise(domainEvent);
        dbContext.Users.Add(user);
        var outboxCommitProcessor = GetService<IOutboxCommitProcessor>();
        var outboxDbContext = GetService<OutboxDbContext>();

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await outboxCommitProcessor.ProcessAndSaveAsync(dbContext);
        });

        // Assert
        var outboxMessage = await outboxDbContext.Set<OutboxMessage>()
        .FirstOrDefaultAsync(x => x.Id == domainEvent.Id);
        Assert.Null(outboxMessage);
    }

    [Fact]
    public async Task ProcessAndSaveAsync_ShouldRollbackIfOutboxMessageFails()
    {
        // Arrange
        var dbContext = GetService<AppDbContext>();
        var outboxDbContext = GetService<OutboxDbContext>();
        var user = new User(Guid.NewGuid(), "John Doe", "John.Doe@Test.com");
        var domainEvent = new UserCreatedDomainEvent(user.Name, user.Email);
        outboxDbContext.Set<OutboxMessage>().Add(new OutboxMessage
        {
            Id = domainEvent.Id,
            EventType = domainEvent.GetType().Name,
            Content = DomainEventJsonConverter.Serialize(domainEvent),
            OccurredOnUtc = domainEvent.OccurredOnUtc,
        });
        outboxDbContext.SaveChanges();
        outboxDbContext.ChangeTracker.Clear();

        user.Raise(domainEvent);
        dbContext.Users.Add(user);
        var outboxCommitProcessor = GetService<IOutboxCommitProcessor>();

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await outboxCommitProcessor.ProcessAndSaveAsync(dbContext);
        });

        // Assert
        var savedUser = await dbContext.Users
        .FirstOrDefaultAsync(x => x.Id == user.Id);
        Assert.Null(savedUser);
    }



}