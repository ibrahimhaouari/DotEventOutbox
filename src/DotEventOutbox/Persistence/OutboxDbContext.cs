using Microsoft.EntityFrameworkCore;

namespace DotEventOutbox.Persistence;

/// <summary>
/// An Entity Framework DbContext tailored for managing outbox-related entities.
/// It plays a key role in handling the persistence, retrieval, and management of outbox messages and related data,
/// ensuring that they are correctly stored and maintained within the database.
/// </summary>
/// <param name="options">The configuration options used by this DbContext, typically provided through dependency injection.</param>
/// <remarks>
/// Initializes a new instance of the <see cref="OutboxDbContext"/> class with the specified DbContext options.
/// </remarks>
/// <param name="options">The options to configure this instance of the DbContext.</param>
internal sealed class OutboxDbContext(DbContextOptions<OutboxDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Optionally specifies a default schema name for the Outbox tables in the database.
    /// When set, this schema name is applied to all tables managed by this DbContext.
    /// </summary>
    public static string? SchemaName { get; set; }

    /// <summary>
    /// Configures the model building for the outbox-related entities.
    /// This method applies entity configurations and sets the default database schema if one is specified.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations from the current assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutboxDbContext).Assembly);

        // If a schema name is specified, apply it as the default schema for all tables.
        if (!string.IsNullOrWhiteSpace(SchemaName))
        {
            modelBuilder.HasDefaultSchema(SchemaName);
        }
    }
}
