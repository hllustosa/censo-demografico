using Census.People.Application.Services;
using Census.People.Domain.Interfaces;
using Census.Shared.Auth;
using Census.Shared.Bus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Census.People.Test.Utils;

public class PeopleWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] = TestJwtHelper.DefaultSigningKey,
                ["Jwt:Issuer"] = "census-identity",
                ["Jwt:Audience"] = "census-api"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IIntegrationEventPublisher>();
            services.RemoveAll<IGuidGenerator>();

            services.AddTransient<IIntegrationEventPublisher, MockIntegrationEventPublisher>();
            services.AddTransient<IGuidGenerator, MockGuidGenerator>();
        });
    }
}

public class MockIntegrationEventPublisher : IIntegrationEventPublisher
{
    public Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

public class MockGuidGenerator : IGuidGenerator
{
    public string GenerateGuid() => "id";
}
