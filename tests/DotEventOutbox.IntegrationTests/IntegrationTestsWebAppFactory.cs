using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using DotEventOutbox.IntegrationTests.WebApp;
using Microsoft.AspNetCore.TestHost;

namespace DotEventOutbox.IntegrationTests;
public class IntegrationTestsWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {

        });
    }
}
