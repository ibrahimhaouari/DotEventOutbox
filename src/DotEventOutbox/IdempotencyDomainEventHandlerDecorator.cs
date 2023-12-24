using System.Transactions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DotEventOutbox.Contracts;
using DotEventOutbox.Persistence;
using DotEventOutbox.Entities;

namespace DotEventOutbox;

/// <summary>
/// A decorator for domain event handlers that ensures idempotent processing of events.
/// This class checks if an event has already been processed by a specific consumer before invoking the handler,
/// thereby preventing duplicate processing and maintaining the integrity of event handling.
/// </summary>
/// <typeparam name="TDomainEvent">The type of the domain event being handled.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="IdempotencyDomainEventHandlerDecorator{TDomainEvent}"/> class.
/// </remarks>
/// <param name="decorated">The actual event handler to be invoked if the event is not a duplicate.</param>
/// <param name="scopeFactory">The factory to create scopes for obtaining services.</param>
/// <param name="logger">Logger for recording execution details.</param>
internal sealed class IdempotencyDomainEventHandlerDecorator<TDomainEvent>(
    INotificationHandler<TDomainEvent> decorated,
    IServiceScopeFactory scopeFactory,
    ILogger<IdempotencyDomainEventHandlerDecorator<TDomainEvent>> logger) : INotificationHandler<TDomainEvent> where TDomainEvent : IEvent
{
    private readonly INotificationHandler<TDomainEvent> decorated = decorated;
    private readonly IServiceScopeFactory scopeFactory = scopeFactory;
    private readonly ILogger<IdempotencyDomainEventHandlerDecorator<TDomainEvent>> logger = logger;

    /// <summary>
    /// Handles the specified domain event ensuring idempotency. It checks for the event's prior processing
    /// before delegating to the decorated handler and records the event's processing upon completion.
    /// </summary>
    /// <param name="notification">The domain event to handle.</param>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(TDomainEvent notification, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();

        var consumer = decorated.GetType().FullName!;

        logger.LogInformation("Handling event: {Event} by consumer: {Consumer}", notification.GetType().Name, consumer);

        // Check if the event has already been processed.
        if (await dbContext.Set<OutboxMessageConsumer>()
            .AnyAsync(m => m.Id == notification.Id && m.Name == consumer, cancellationToken))
        {
            logger.LogInformation("Event: {Event} has already been processed by consumer: {Consumer}", notification.GetType().Name, consumer);
            return;
        }

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        // Handle the event.
        await decorated.Handle(notification, cancellationToken);
        // Record that the event has been processed.
        dbContext.Set<OutboxMessageConsumer>().Add(new OutboxMessageConsumer
        {
            Id = notification.Id,
            Name = consumer
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Event: {Event} processed successfully by consumer: {Consumer}", notification.GetType().Name, consumer);
        transaction.Complete();
    }
}
