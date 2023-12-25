using DotEventOutbox.IntegrationTests.WebApp;
using DotEventOutbox.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace DotEventOutbox.IntegrationTests;

public abstract class BaseIntegrationTest(IntegrationTestsWebAppFactory factory) : IClassFixture<IntegrationTestsWebAppFactory>
{
    private readonly IServiceScope scope = factory.Services.CreateScope();

    protected T GetService<T>() where T : notnull
    {
        return scope.ServiceProvider.GetRequiredService<T>();
    }
}