using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using DotEventOutbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

using var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string schemaName = "Outbox";
        services.AddOutbox(configuration,
        options => options.UseNpgsql(configuration.GetConnectionString("AppDb"),
                o => o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, schemaName)),
                schemaName);
    }).Build();


