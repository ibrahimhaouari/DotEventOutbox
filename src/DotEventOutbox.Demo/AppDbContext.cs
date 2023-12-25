using Microsoft.EntityFrameworkCore;

namespace DotEventOutbox.Demo;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;

    protected override async void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        await Database.MigrateAsync();
    }
}