using Microsoft.EntityFrameworkCore;

namespace DotEventOutbox;

/// <summary>
/// Defines the contract for a service that processes and saves domain events as outbox messages.
/// </summary>
public interface IOutboxCommitProcessor
{
    /// <summary>
    /// Processes domain events from the specified DbContext, converts them to outbox messages,
    /// and saves them to the database.
    /// </summary>
    /// <param name="dbContext">The DbContext from which to extract and process domain events.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task ProcessAndSaveAsync(DbContext dbContext, CancellationToken cancellationToken = default);
}
