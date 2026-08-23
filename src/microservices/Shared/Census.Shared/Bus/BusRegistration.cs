using Census.Shared.Bus.Implementation;
using Census.Shared.Bus.Interfaces;
using Census.Shared.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Census.Shared.Bus
{
    public static class BusRegistration
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddConnectionFactory(configuration);
            services.AddSingleton<IEventBusSubscriptionsManager, RabbitMQSubscriptionManager>();
            services.AddSingleton<EventBusMetrics>();
            services.AddSingleton<IEventBus, RabbitMQEventBus>();
            return services;
        }

        public static IServiceCollection AddConnectionFactory(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IPersistentConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<PersistentConnection>>();
                var rabbitMqConfig = configuration.GetSection("RabbitMqConnection");

                var factory = new ConnectionFactory
                {
                    HostName = rabbitMqConfig["HostName"] ?? "localhost",
                    UserName = rabbitMqConfig["Username"] ?? "guest",
                    Password = rabbitMqConfig["Password"] ?? "guest",
                    DispatchConsumersAsync = true
                };

                var retryCount = int.Parse(rabbitMqConfig["retryCount"] ?? "5");
                return new PersistentConnection(factory, logger, retryCount);
            });

            return services;
        }
    }
}
