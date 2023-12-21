using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using DotEventOutbox.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

using var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        var configuration = new ConfigurationBuilder()
            //set current project as base path
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        services.AddOutbox(configuration,
        options => options.UseNpgsql(configuration.GetConnectionString("AppDb"),
                o => o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "Outbox")),
        autoMigrate: true // Migrate database
    );
    }).Build();


