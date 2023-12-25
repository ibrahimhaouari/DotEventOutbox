using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using DotEventOutbox.IntegrationTests.WebApp;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using DotEventOutbox.Persistence;
using Testcontainers.PostgreSql;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DotEventOutbox.IntegrationTests;
public class IntegrationTestsWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("DotEventOutboxTest")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public Task InitializeAsync()
    {
        return _dbContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            services.RemoveAll(typeof(DbContextOptions<OutboxDbContext>));
            services.AddDbContext<OutboxDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString(), builder =>
                {
                    builder.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName
                        , OutboxDbContext.SchemaName);
                });
            });
        });
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return _dbContainer.StopAsync();
    }
}
