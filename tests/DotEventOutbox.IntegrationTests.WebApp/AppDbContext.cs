using Microsoft.EntityFrameworkCore;

namespace DotEventOutbox.IntegrationTests.WebApp;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        Database.MigrateAsync();
    }
}