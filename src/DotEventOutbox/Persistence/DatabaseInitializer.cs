using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotEventOutbox.Persistence;

/// <summary>
/// Provides essential functionality for initializing the database used by the Outbox system.
/// This includes applying pending Entity Framework migrations to ensure the database structure
/// is up-to-date and aligned with the current model definitions.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Applies any pending migrations to the Outbox database context to ensure the database schema is current.
    /// This method is typically called during application startup or deployment to align the database schema with the latest model changes.
    /// </summary>
    /// <param name="serviceProvider">The service provider that holds services configured for the application, including the OutboxDbContext.</param>
    /// <exception cref="InvalidOperationException">Thrown if the OutboxDbContext cannot be resolved from the service provider, indicating a configuration issue.</exception>
    public static void ApplyMigrations(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>()
            ?? throw new InvalidOperationException("OutboxDbContext is not found in the service provider.");

        dbContext.Database.Migrate();
    }
}
