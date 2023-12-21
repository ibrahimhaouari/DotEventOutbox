using System.Transactions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DotEventOutbox.Entities;
using DotEventOutbox.Contracts;

namespace DotEventOutbox.Infrastructure.EntityFramework;

/// <summary>
/// Decorator for domain event handlers to ensure idempotent processing of events.
/// It checks if an event has already been processed before invoking the handler.
/// </summary>
/// <typeparam name="TDomainEvent">The type of the domain event.</typeparam>
internal sealed class IdempotencyDomainEventHandlerDecorator<TDomainEvent>(
    INotificationHandler<TDomainEvent> decorated,
    IServiceScopeFactory scopeFactory,
    ILogger<IdempotencyDomainEventHandlerDecorator<TDomainEvent>> logger)
    : INotificationHandler<TDomainEvent> where TDomainEvent : IEvent
{
    private readonly INotificationHandler<TDomainEvent> decorated = decorated;
    private readonly IServiceScopeFactory scopeFactory = scopeFactory;
    private readonly ILogger<IdempotencyDomainEventHandlerDecorator<TDomainEvent>> logger = logger;

    public async Task Handle(TDomainEvent notification, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();

        var consumer = decorated.GetType().FullName!;

        logger.LogInformation("Handling event: {Event} by consumer: {Consumer}", notification.GetType().Name, consumer);

        // Check if the event has already been processed.
        if (await dbContext.Set<OutboxMessagesConsumer>()
            .AnyAsync(m => m.Id == notification.Id && m.Name == consumer, cancellationToken))
        {
            logger.LogInformation("Event: {Event} has already been processed by consumer: {Consumer}", notification.GetType().Name, consumer);
            return;
        }

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        // Handle the event.
        await decorated.Handle(notification, cancellationToken);
        // Record that the event has been processed.
        dbContext.Set<OutboxMessagesConsumer>().Add(new OutboxMessagesConsumer
        {
            Id = notification.Id,
            Name = consumer
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Event: {Event} processed successfully by consumer: {Consumer}", notification.GetType().Name, consumer);
        transaction.Complete();
    }
}
