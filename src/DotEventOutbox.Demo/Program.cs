using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using DotEventOutbox;
using Microsoft.EntityFrameworkCore;
using DotEventOutbox.Demo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Create and configure the host
var host = Host.CreateDefaultBuilder()
.ConfigureServices(services =>
{
    // Build configuration from appsettings.json
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    // configure logging
    services.AddLogging(builder => builder.AddConfiguration(configuration.GetSection("Logging")));

    // Register the application's DbContext
    services.AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(configuration.GetConnectionString("AppDb"));
    });

    // Register domain event handlers using MediatR
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));


    // Register DotEventOutbox services
    services.AddDotEventOutbox(configuration,
        options => options.UseNpgsql(configuration.GetConnectionString("AppDb")));

}).Build();

await host.MigrateDotEventOutbox();

// Create a scope for the services
using var scope = host.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await dbContext.Database.MigrateAsync();

// Create a new user instance
var user = new User(Guid.NewGuid(), "John Doe", "John.Doe@Demo.com");

// Add the new user to the DbContext
dbContext.Users.Add(user);

// Save changes and process domain events using OutboxCommitProcessor
var outboxCommitProcessor = scope.ServiceProvider.GetRequiredService<IOutboxCommitProcessor>();
await outboxCommitProcessor.ProcessAndSaveAsync(dbContext);

// Confirmation message after successful operation
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("User created successfully.");
Console.ResetColor();

// Run the host
await host.RunAsync();
