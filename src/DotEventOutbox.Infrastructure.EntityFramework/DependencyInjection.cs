using DotEventOutbox.Infrastructure.Common.Configuration;
using DotEventOutbox.Infrastructure.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace inteliQo.Outbox.Infrastructure.EntityFramework;

/// <summary>
/// Extension methods for configuring outbox-related services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the outbox services to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="optionsAction">An action to configure the database context options.</param>
    /// <param name="autoMigrate">Specifies whether to automatically apply database migrations on startup.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration, Action<DbContextOptionsBuilder> optionsAction, bool autoMigrate = false)
    {
        // Configure Outbox settings and get the instance of the settings
        var outboxSettings = services.ConfigureOutboxSettings(configuration);

        // Add OutboxDbContext to the services
        services.AddOutboxDbContext(optionsAction)
                // Add DomainEventsToOutboxMessagesConverter to the services
                .AddDomainEventsConverter()
                // Configure Quartz with the outboxSettings
                .ConfigureQuartz(outboxSettings)
                // Decorate INotificationHandler
                .DecorateNotificationHandlers();


        // Migrate database on startup
        if (autoMigrate)
        {
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
            DatabaseInitializer.ApplyMigrations(dbContext);
        }

        return services;
    }

    // Add OutboxDbContext to the services
    private static IServiceCollection AddOutboxDbContext(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
    {
        services.AddDbContext<OutboxDbContext>(optionsAction);
        return services;
    }

    // Configure and add OutboxSettings to the services
    private static EventOutboxConfiguration ConfigureOutboxSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var eventOutboxConfiguration = new EventOutboxConfiguration();
        configuration.Bind(EventOutboxConfiguration.SectionName, eventOutboxConfiguration);
        services.AddSingleton(Options.Create(eventOutboxConfiguration));
        return eventOutboxConfiguration;
    }

    // Add DomainEventsToOutboxMessagesConverter to the services
    private static IServiceCollection AddDomainEventsConverter(this IServiceCollection services)
    {
        services.AddScoped<EventsToOutboxMessagesConverter>();
        return services;
    }

    // Configure Quartz services
    private static IServiceCollection ConfigureQuartz(this IServiceCollection services, EventOutboxConfiguration configuration)
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
        services.Decorate(typeof(INotificationHandler<>), typeof(IdempotencyDomainEventHandlerDecorator<>));
        return services;
    }
}
