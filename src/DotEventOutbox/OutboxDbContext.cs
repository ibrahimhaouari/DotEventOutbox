using Microsoft.EntityFrameworkCore;

namespace DotEventOutbox;

/// <summary>
/// Entity Framework DbContext for managing outbox-related entities.
/// This context is responsible for handling the persistence of outbox messages and related data.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="OutboxDbContext"/> class.
/// </remarks>
/// <param name="options">The options to be used by the DbContext.</param>
public class OutboxDbContext(DbContextOptions<OutboxDbContext> options) : DbContext(options)
{
    /// <summary>
    /// The default schema name for the Outbox tables in the database.
    /// </summary>
    public static string? SchemaName { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutboxDbContext).Assembly);

        // Apply schema name if specified
        if (!string.IsNullOrWhiteSpace(SchemaName))
        {
            modelBuilder.HasDefaultSchema(SchemaName);
        }
    }
}
