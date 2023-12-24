using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using DotEventOutbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using DotEventOutbox.Demo;
using Microsoft.Extensions.DependencyInjection;

var host = Host.CreateDefaultBuilder()
.ConfigureServices(services =>
   {
       var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

       //Register DbContext
       services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("AppDb"));
        });

       //Register Event Handlers
       services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

       string schemaName = "Outbox";
       services.AddOutbox(configuration,
        options => options.UseNpgsql(configuration.GetConnectionString("AppDb"),
                o => o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, schemaName)),
                schemaName);

   }).Build();



//Create scope
using var scope = host.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

//Migrate database
await dbContext.Database.MigrateAsync();

//Create User
var user = new User(Guid.NewGuid(), "John Doe", "John.Doe@Demo.com");
//Add user to DbContext
dbContext.Users.Add(user);

//Save changes with events
var outboxCommitProcessor = scope.ServiceProvider.GetRequiredService<OutboxCommitProcessor>();
await outboxCommitProcessor.ProcessAndSaveAsync(dbContext);
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("User created successfully.");
Console.ResetColor();

//Run host
await host.RunAsync();




