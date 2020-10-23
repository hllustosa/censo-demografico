using Census.Shared.Bus.Implementation;
using Census.Shared.Bus.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Census.Shared.Bus
{
    public static class BusRegistration
    {
        public static void AddEventBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddConnectionFactory(configuration);
            services.AddTransient<IEventBusSubscriptionsManager, RabbitMQSubscriptionManager>();
            services.AddTransient<IEventBus, RabbitMQEventBus>();
        }

        public static void AddConnectionFactory(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IPersistentConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<PersistentConnection>>();
                var rabbitMQConfig = configuration.GetSection("RabbitMqConnection");

                var factory = new ConnectionFactory()
                {
                    HostName = rabbitMQConfig["HostName"],
                    UserName = rabbitMQConfig["Username"],
                    Password = rabbitMQConfig["Password"],
                    DispatchConsumersAsync = true
                };

                var retryCount = int.Parse(rabbitMQConfig["retryCount"]);
                return new PersistentConnection(factory, logger, retryCount);
            });
        }
    }
}
