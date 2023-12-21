using DotEventOutbox.Infrastructure.Common.Configuration;
using Microsoft.EntityFrameworkCore;

namespace DotEventOutbox.Infrastructure.EntityFramework;

public class OutboxDbContext(DbContextOptions<OutboxDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply entity configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutboxDbContext).Assembly);

        // Set the default schema for the database context
        modelBuilder.HasDefaultSchema(EventOutboxConfiguration.SectionName);

        base.OnModelCreating(modelBuilder);
    }
}
