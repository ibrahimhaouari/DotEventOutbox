using DotEventOutbox.Contracts;
using DotEventOutbox.Entities;
using DotEventOutbox.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotEventOutbox.UnitTests;

public class IdempotentDomainEventHandlerDecoratorTests
{

    // Factory method to create a new in-memory OutboxDbContext
    private static OutboxDbContext GetOutboxDbContext() =>
        new(new DbContextOptionsBuilder<OutboxDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options);

    public record MockDomainEvent() : DomainEvent;

    [Fact]
    public async Task Handle_InvokesDecoratedHandler_WhenEventIsNotDuplicate()
    {
        // Arrange: Set up OutboxDbContext and IdempotentDomainEventHandlerDecorator
        var outboxDbContext = GetOutboxDbContext();
        // Mock the IServiceScope
        var serviceScope = new Mock<IServiceScope>();
        serviceScope.Setup(s => s.ServiceProvider.GetService(typeof(OutboxDbContext)))
                    .Returns(outboxDbContext);
        // Mock the IServiceScopeFactory
        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory.Setup(s => s.CreateScope()).Returns(serviceScope.Object);

        var decorated = new Mock<INotificationHandler<MockDomainEvent>>();
        var decorator = new IdempotentDomainEventHandlerDecorator<MockDomainEvent>(
            decorated.Object,
            scopeFactory.Object,
            new Mock<ILogger<IdempotentDomainEventHandlerDecorator<MockDomainEvent>>>().Object);

        // Create a mock domain event and corresponding OutboxMessage
        var domainEvent = new MockDomainEvent();

        // Act: Invoke the decorator's Handle method
        await decorator.Handle(domainEvent, CancellationToken.None);

        // Assert: Verify that the decorated handler was invoked
        decorated.Verify(d => d.Handle(domainEvent, CancellationToken.None), Times.Once);
        var outboxMessageConsumers = await outboxDbContext.Set<OutboxMessageConsumer>().ToListAsync();
        Assert.Single(outboxMessageConsumers);
        Assert.Equal(decorated.Object.GetType().FullName, outboxMessageConsumers.First().Name);
        Assert.Equal(domainEvent.Id, outboxMessageConsumers.First().Id);
    }

    [Fact]
    public async Task Handle_DoesNotInvokeDecoratedHandler_WhenEventIsDuplicate()
    {
        // Arrange: Set up OutboxDbContext and IdempotentDomainEventHandlerDecorator
        var outboxDbContext = GetOutboxDbContext();
        // Mock the IServiceScope
        var serviceScope = new Mock<IServiceScope>();
        serviceScope.Setup(s => s.ServiceProvider.GetService(typeof(OutboxDbContext)))
                    .Returns(outboxDbContext);
        // Mock the IServiceScopeFactory
        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory.Setup(s => s.CreateScope()).Returns(serviceScope.Object);

        var decorated = new Mock<INotificationHandler<MockDomainEvent>>();
        var decorator = new IdempotentDomainEventHandlerDecorator<MockDomainEvent>(
            decorated.Object,
            scopeFactory.Object,
            new Mock<ILogger<IdempotentDomainEventHandlerDecorator<MockDomainEvent>>>().Object);

        // Create a mock domain event and corresponding OutboxMessage
        var domainEvent = new MockDomainEvent();
        await outboxDbContext.Set<OutboxMessageConsumer>().AddAsync(new OutboxMessageConsumer
        {
            Id = domainEvent.Id,
            Name = decorated.Object.GetType().FullName!
        });
        await outboxDbContext.SaveChangesAsync();

        // Act: Invoke the decorator's Handle method
        await decorator.Handle(domainEvent, CancellationToken.None);

        // Assert: Verify that the decorated handler was not invoked
        decorated.Verify(d => d.Handle(domainEvent, CancellationToken.None), Times.Never);
        var outboxMessageConsumers = await outboxDbContext.Set<OutboxMessageConsumer>().ToListAsync();
        Assert.Single(outboxMessageConsumers);
        Assert.Equal(decorated.Object.GetType().FullName, outboxMessageConsumers.First().Name);
        Assert.Equal(domainEvent.Id, outboxMessageConsumers.First().Id);
    }
}