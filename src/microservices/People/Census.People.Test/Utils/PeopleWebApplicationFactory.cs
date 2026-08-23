using Census.People.Application.Services;
using Census.People.Domain.Interfaces;
using Census.Shared.Auth;
using Census.Shared.Bus;
using Census.Shared.Bus.Implementation;
using Census.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Census.People.Test.Utils;

public class PeopleWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _mongoConnectionString;

    public PeopleWebApplicationFactory(string mongoConnectionString)
    {
        _mongoConnectionString = mongoConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _mongoConnectionString,
                ["Jwt:SigningKey"] = TestJwtHelper.DefaultSigningKey,
                ["Jwt:Issuer"] = "census-identity",
                ["Jwt:Audience"] = "census-api",
                ["RabbitMqConnection:PublisherOnly"] = "true",
                ["RabbitMqConnection:HostName"] = "localhost"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IIntegrationEventPublisher>();
            services.RemoveAll<IGuidGenerator>();

            // OutboxProcessor opens RabbitMQ; integration tests publish via the mock publisher only.
            foreach (var descriptor in services.Where(d =>
                         d.ServiceType == typeof(IHostedService) &&
                         d.ImplementationType == typeof(OutboxProcessor)).ToList())
            {
                services.Remove(descriptor);
            }

            services.AddTransient<IIntegrationEventPublisher, MockIntegrationEventPublisher>();
            services.AddTransient<IGuidGenerator, MockGuidGenerator>();
        });
    }
}

public class MockIntegrationEventPublisher : IIntegrationEventPublisher
{
    public Task PublishAsync(
        IntegrationEvent integrationEvent,
        ITransaction? transaction = null,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

public class MockGuidGenerator : IGuidGenerator
{
    public string GenerateGuid() => "id";
}
