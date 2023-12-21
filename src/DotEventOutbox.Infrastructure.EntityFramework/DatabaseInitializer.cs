using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotEventOutbox.Infrastructure.EntityFramework;

/// <summary>
/// Provides functionality to initialize the database used by the Outbox system,
/// including applying any pending Entity Framework migrations.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Applies pending migrations to the Outbox database context.
    /// </summary>
    /// <param name="serviceProvider">The service provider containing the OutboxDbContext.</param>
    /// <exception cref="InvalidOperationException">Thrown if OutboxDbContext is not found.</exception>
    public static void ApplyMigrations(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>()
            ?? throw new InvalidOperationException("OutboxDbContext is not found in the service provider.");

        dbContext.Database.Migrate();
    }
}