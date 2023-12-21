using System.Transactions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DotEventOutbox.Entities;
using DotEventOutbox.Contracts;

namespace DotEventOutbox.Infrastructure.EntityFramework;

/// <summary>
/// Decorator that adds idempotency handling to the event handling process.
/// </summary>
/// <typeparam name="TDomainEvent">Type of the domain event.</typeparam>
public sealed class IdempotencyDomainEventHandlerDecorator<TDomainEvent> : INotificationHandler<TDomainEvent>
    where TDomainEvent : IEvent
{
    private readonly INotificationHandler<TDomainEvent> _decorated;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<IdempotencyDomainEventHandlerDecorator<TDomainEvent>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdempotencyEventHandlerDecorator{TDomainEvent}"/> class.
    /// </summary>
    /// <param name="decorated">The decorated event handler.</param>
    /// <param name="scopeFactory">The service scope factory.</param>
    /// <param name="logger">The logger.</param>
    public IdempotencyDomainEventHandlerDecorator(
        INotificationHandler<TDomainEvent> decorated,
        IServiceScopeFactory scopeFactory,
        ILogger<IdempotencyDomainEventHandlerDecorator<TDomainEvent>> logger)
    {
        _decorated = decorated;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Handles the domain event with idempotency handling.
    /// </summary>
    /// <param name="notification">The domain event notification.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(TDomainEvent notification, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();

        var consumer = _decorated.GetType().FullName;

        _logger.LogInformation("Handling event: {Event} by consumer: {Consumer}", notification.GetType().Name, consumer);

        // If the event has already been processed by this handler, do nothing.
        if (await dbContext.Set<OutboxMessagesConsumer>()
            .AnyAsync(m => m.Id == notification.Id && m.Name == consumer, cancellationToken))
        {
            _logger.LogInformation("Event: {Event} has already been processed by consumer: {Consumer}", notification.GetType().Name, consumer);
            return;
        }

        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            // Handle the event.
            await _decorated.Handle(notification, cancellationToken);

            // Record that the event has been processed by this handler.
            dbContext.Set<OutboxMessagesConsumer>().Add(new OutboxMessagesConsumer
            {
                Id = notification.Id,
                Name = consumer!
            });

            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Event: {Event} has been processed successfully by consumer: {Consumer}", notification.GetType().Name, consumer);

            transaction.Complete();
        }
    }
}
