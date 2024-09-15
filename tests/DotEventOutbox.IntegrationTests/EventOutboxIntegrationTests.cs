using DotEventOutbox.Entities;
using DotEventOutbox.IntegrationTests.WebApp;
using DotEventOutbox.Persistence;
using DotEventOutbox.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;

namespace DotEventOutbox.IntegrationTests;

public class EventOutboxIntegrationTests(IntegrationTestsWebAppFactory factory) : BaseIntegrationTest(factory)
{

    [Fact]
    // Test that the decorator does not handle the same domain event twice
    public async Task EventProcessing_ShouldMoveToDeadLetterAndSaveOneConsumer_WhenSecondConsumerFails()
    {
        // Arrange
        var dbContext = GetService<AppDbContext>();
        var outboxDbContext = GetService<OutboxDbContext>();
        var user = new User(Guid.NewGuid(), "John Doe", "John.Doe@Test.com");
        var domainEvent = new UserCreatedDomainEvent(user.Name, user.Email);
        user.RaiseEvent(domainEvent);
        dbContext.Users.Add(user);
        var outboxCommitProcessor = GetService<IOutboxCommitProcessor>();
        var settings = GetService<IOptions<EventOutboxSettings>>().Value;


        // Act
        await outboxCommitProcessor.ProcessAndSaveAsync(dbContext);
        // wait for the job to process the outbox
        await Task.Delay(settings.ProcessingIntervalInSeconds * 1000);
        await Task.Delay(settings.RetryIntervalInMilliseconds * 3);

        // Assert
        var outboxMessage = await outboxDbContext.Set<OutboxMessage>().FirstOrDefaultAsync(m => m.Id == domainEvent.Id);
        Assert.Null(outboxMessage);

        var consumers = await outboxDbContext.Set<OutboxMessageConsumer>().Where(c => c.Id == domainEvent.Id).ToListAsync();
        Assert.NotEmpty(consumers);
        Assert.Single(consumers);
        Assert.Equal(typeof(UserCreatedSendEmailHandler).FullName, consumers[0].Name);

        var deadLetterMessage = await outboxDbContext.Set<DeadLetterMessage>().FirstOrDefaultAsync(m => m.Id == domainEvent.Id);
        Assert.NotNull(deadLetterMessage);
        Assert.Equal(domainEvent.Id, deadLetterMessage.Id);
        Assert.Equal(deadLetterMessage.RetryCount, settings.MaxRetryAttempts);
    }

    [Fact]
    public async Task OutboxMessageProcessingJob_ShouldBeConfiguredCorrectly()
    {
        // Arrange
        var jobKey = JobKey.Create(nameof(OutboxMessageProcessingJob));
        var schedulers = await GetService<ISchedulerFactory>().GetAllSchedulers();
        var scheduler = schedulers.FirstOrDefault();
        var job = scheduler != null ? await scheduler.GetJobDetail(jobKey) : null;

        // Assert
        Assert.NotNull(scheduler);
        Assert.NotNull(job);
        Assert.True(scheduler.IsStarted);
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
        user.RaiseEvent(domainEvent);
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
    public async Task ProcessAndSaveAsync_ShouldRollbackIfOutboxMessagesSaveFails()
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

        user.RaiseEvent(domainEvent);
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