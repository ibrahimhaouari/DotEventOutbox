using DotEventOutbox.Infrastructure.Common.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace DotEventOutbox.Infrastructure.EntityFramework;

public static class DependencyInjection
{
    public static IServiceCollection AddOutbox(this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder> optionsAction,
        string? schemaName = null)
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

        // apply migrations
        DatabaseInitializer.ApplyMigrations(services.BuildServiceProvider());

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
    private static IServiceCollection AddOutboxCommitProcessor(this IServiceCollection services)
    {
        services.AddScoped<OutboxCommitProcessor>();
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
        if (services.Any(x => x.ServiceType == typeof(INotificationHandler<>)))
        {
            services.Decorate(typeof(INotificationHandler<>), typeof(IdempotencyDomainEventHandlerDecorator<>));
        }
        return services;
    }
}
