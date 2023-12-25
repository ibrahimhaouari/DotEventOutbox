using DotEventOutbox.IntegrationTests.WebApp;
using Microsoft.Extensions.DependencyInjection;

namespace DotEventOutbox.IntegrationTests;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestsWebAppFactory>
{
    protected readonly IServiceScope scope;
    protected readonly AppDbContext dbContext;
    protected readonly IOutboxCommitProcessor outboxCommitProcessor;

    protected BaseIntegrationTest(IntegrationTestsWebAppFactory factory)
    {
        scope = factory.Services.CreateScope();
        dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        outboxCommitProcessor = scope.ServiceProvider.GetRequiredService<IOutboxCommitProcessor>();
    }
}