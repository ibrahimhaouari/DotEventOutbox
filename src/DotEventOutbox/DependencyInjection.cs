using DotEventOutbox.Persistence;
using DotEventOutbox.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace DotEventOutbox;

public static class DependencyInjection
{
    /// <summary>
    /// Adds and configures services required by DotEventOutbox to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configuration">Configuration for the application.</param>
    /// <param name="optionsAction">An action to configure the DbContextOptions for OutboxDbContext.</param>
    /// <param name="schemaName">Optional schema name for database tables.</param>
    /// <returns>The IServiceCollection for chaining.</returns>
    public static IServiceCollection AddDotEventOutbox(this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder> optionsAction,
        string schemaName = "outbox")
    {
        // Configure Outbox settings and get the instance of the settings
        var outboxSettings = services.ConfigureOutboxSettings(configuration);

        // Configure SchemaName
        OutboxDbContext.SchemaName = schemaName;

        // Add OutboxDbContext to the services
        services.AddOutboxDbContext(optionsAction)
                // Add AddOutboxCommitProcessor to the services
                .AddOutboxCommitProcessor()
                // Configure Quartz with the outboxSettings
                .ConfigureQuartz(outboxSettings)
                // Decorate INotificationHandler
                .DecorateNotificationHandlers();

        return services;
    }

    // Add OutboxDbContext to the services
    private static IServiceCollection AddOutboxDbContext(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
    {
        services.AddDbContext<OutboxDbContext>(optionsAction);
        return services;
    }

    // Configure and add OutboxSettings to the services
    private static EventOutboxSettings ConfigureOutboxSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var eventOutboxConfiguration = new EventOutboxSettings();
        configuration.Bind(EventOutboxSettings.SectionName, eventOutboxConfiguration);
        services.AddSingleton(Options.Create(eventOutboxConfiguration));
        return eventOutboxConfiguration;
    }

    // Add IOutboxCommitProcessor to the services
    private static IServiceCollection AddOutboxCommitProcessor(this IServiceCollection services)
    {
        services.AddScoped<IOutboxCommitProcessor, OutboxCommitProcessor>();
        return services;
    }

    // Configure Quartz services
    private static IServiceCollection ConfigureQuartz(this IServiceCollection services, EventOutboxSettings configuration)
    {
        services.AddQuartz(config =>
        {
            var jobKey = JobKey.Create(nameof(OutboxMessageProcessingJob));
            config.AddJob<OutboxMessageProcessingJob>(jobKey)
            .AddTrigger(trigger => trigger
                .ForJob(jobKey)
                .WithSimpleSchedule(schedule => schedule
                    .WithIntervalInSeconds(configuration.ProcessingIntervalInSeconds)
                    .RepeatForever()
                ));

        });
        services.AddQuartzHostedService(
            q => q.WaitForJobsToComplete = true
        );

        return services;
    }

    // Decorate INotificationHandler with IdempotentDomainEventHandler
    private static IServiceCollection DecorateNotificationHandlers(this IServiceCollection services)
    {
        services.TryDecorate(typeof(INotificationHandler<>), typeof(IdempotentDomainEventHandlerDecorator<>));
        return services;
    }
}
