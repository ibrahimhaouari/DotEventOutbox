using System;
using Xunit;
using DotEventOutbox.Contracts;
using DotEventOutbox.Entities;
using DotEventOutbox.Persistence;
using DotEventOutbox.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Quartz;

namespace DotEventOutbox.Tests;

public class OutboxMessageProcessingJobTests
{
    // Factory method to create a new in-memory OutboxDbContext
    private static OutboxDbContext GetOutboxDbContext() =>
        new(new DbContextOptionsBuilder<OutboxDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options);

    // Mock domain event for testing
    private record MockDomainEvent() : DomainEvent;

    [Fact]
    public async Task Execute_PublishesDomainEvents_WhenMessagesArePending()
    {
        // Arrange: Set up OutboxDbContext, IPublisher mock, and OutboxMessageProcessingJob
        var outboxDbContext = GetOutboxDbContext();
        var publisherMock = new Mock<IPublisher>();
        var job = new OutboxMessageProcessingJob(
            outboxDbContext,
            publisherMock.Object,
            Options.Create(new EventOutboxSettings()),
            new Mock<ILogger<OutboxMessageProcessingJob>>().Object);

        // Create a mock domain event and corresponding OutboxMessage
        var domainEvent = new MockDomainEvent();
        var message = new OutboxMessage
        {
            Id = domainEvent.Id,
            EventType = domainEvent.GetType().Name,
            Content = JsonConvert.SerializeObject(domainEvent, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }),
            OccurredOnUtc = domainEvent.OccurredOnUtc,
        };

        // Add the message to the OutboxDbContext and save changes
        outboxDbContext.Set<OutboxMessage>().Add(message);
        await outboxDbContext.SaveChangesAsync();

        // Act: Execute the job with a mock IJobExecutionContext
        await job.Execute(new Mock<IJobExecutionContext>().Object);

        // Assert: Verify that the Publish method was called and the message's ProcessedOnUtc is set
        publisherMock.Verify(p => p.Publish(It.IsAny<INotification>(), default), Times.Once);
        var processedMessage = await outboxDbContext.Set<OutboxMessage>().FindAsync(message.Id);
        Assert.NotNull(processedMessage?.ProcessedOnUtc);
    }

    [Fact]
    public async Task Execute_MovesMessagesToDeadLetterQueue_OnFailure()
    {
        // Arrange
        var outboxDbContext = GetOutboxDbContext();
        var publisherMock = new Mock<IPublisher>();
        // Setup the publisher mock to throw an exception when Publish is called
        var exception = new InvalidOperationException("Publishing failed");
        publisherMock.Setup(p => p.Publish(It.IsAny<INotification>(), default))
                     .ThrowsAsync(exception);

        var job = new OutboxMessageProcessingJob(
            outboxDbContext,
            publisherMock.Object,
            Options.Create(new EventOutboxSettings { MaxRetryAttempts = 3, RetryIntervalInMilliseconds = 50 }),
            new Mock<ILogger<OutboxMessageProcessingJob>>().Object);

        var domainEvent = new MockDomainEvent();
        var message = new OutboxMessage
        {
            Id = domainEvent.Id,
            EventType = domainEvent.GetType().Name,
            Content = JsonConvert.SerializeObject(domainEvent, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }),
            OccurredOnUtc = domainEvent.OccurredOnUtc,
        };

        outboxDbContext.Set<OutboxMessage>().Add(message);
        await outboxDbContext.SaveChangesAsync();

        // Act
        await job.Execute(new Mock<IJobExecutionContext>().Object);

        // Assert
        // Verify that the message is moved to the dead letter queue
        var deadLetterMessage = await outboxDbContext.Set<DeadLetterMessage>().FindAsync(message.Id);
        Assert.NotNull(deadLetterMessage);
        Assert.Equal(message.Id, deadLetterMessage.Id);
        Assert.Equal(exception.ToString(), deadLetterMessage.Error);
        Assert.Equal(3, deadLetterMessage.RetryCount); // Assuming MaxRetryAttempts is set to 3 in EventOutboxSettings
    }

}
