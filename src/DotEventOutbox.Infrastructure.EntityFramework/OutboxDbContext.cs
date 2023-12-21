using Microsoft.EntityFrameworkCore;

namespace DotEventOutbox.Infrastructure.EntityFramework;

/// <summary>
/// Represents the database context for the Outbox module.
/// </summary>
public class OutboxDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxDbContext"/> class.
    /// </summary>
    /// <param name="options">The options for configuring the database context.</param>
    public OutboxDbContext(DbContextOptions<OutboxDbContext> options) : base(options)
    { }

    /// <summary>
    /// Configures the database schema and applies entity configurations from the assembly.
    /// </summary>
    /// <param name="modelBuilder">The model builder instance used to configure the database context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply entity configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutboxDbContext).Assembly);

        // Set the default schema for the database context
        modelBuilder.HasDefaultSchema("Outbox");

        base.OnModelCreating(modelBuilder);
    }
}
