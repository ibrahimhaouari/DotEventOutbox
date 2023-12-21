using Microsoft.EntityFrameworkCore;

namespace DotEventOutbox.Infrastructure.EntityFramework;

/// <summary>
/// Helper class for applying migrations to the outbox database.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Applies pending migrations to the outbox database.
    /// </summary>
    /// <param name="context">The instance of <see cref="OutboxDbContext"/>.</param>
    public static void ApplyMigrations(OutboxDbContext context)
    {
        context.Database.Migrate();
    }
}
