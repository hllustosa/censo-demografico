using Census.Shared.Bus.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Census.Shared.Bus.Implementation
{
    public class RabbitMQEventBus : IEventBus
    {
        private static readonly byte PERSISTENT_MODE = 2;
        private static readonly string CENSUS_EXCHANGE = "census";

        private readonly IPersistentConnection PersistentConnection;
        private readonly IEventBusSubscriptionsManager SubscriptionManager;
        private readonly ILogger<RabbitMQEventBus> Logger;
        private readonly IService­Provider Service­Provider;
        private readonly String QueueName;
        private readonly int RetryCount;

        private IModel ConsumerChannel;

        public RabbitMQEventBus(IPersistentConnection persistentConnection, 
            IEventBusSubscriptionsManager subscriptionManager,
            ILogger<RabbitMQEventBus> logger,
            IConfiguration configuration,
            IService­Provider service­Provider,
            int retryCount = 5)
        {
            QueueName = configuration.GetSection("RabbitMqConnection")["QueueName"];
            PersistentConnection = persistentConnection;
            SubscriptionManager = subscriptionManager;
            Service­Provider = service­Provider;
            Logger = logger;
            RetryCount = retryCount;
            ConsumerChannel = CreateConsumerChannel();
        }

        public async void Publish(IntegrationEvent @event)
        {
            ConnectToBroker();
            var policy = CreateRetryPolicy(@event);

            var eventName = @event.GetType().Name;
            using (var channel = PersistentConnection.CreateModel())
            {
                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);

                await policy.ExecuteAsync(() =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = PERSISTENT_MODE;

                    Logger.LogTrace("Publicando evento no RabbitMQ: {EventId}", @event.Id);

                    channel.BasicPublish(
                        exchange: CENSUS_EXCHANGE,
                        routingKey: eventName,
                        mandatory: true,
                        basicProperties: properties,
                        body: body);

                    return Task.CompletedTask;
                });
            }
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            SubscriptionManager.AddSubscription<T, TH>();
            StartBasicConsume();
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = SubscriptionManager.GetEventKey<T>();
            Logger.LogInformation("Unsubscribing from event {EventName}", eventName);
            SubscriptionManager.RemoveSubscription<T, TH>();
        }

        private void StartBasicConsume()
        {
            Logger.LogTrace("Starting RabbitMQ basic consume");

            if (ConsumerChannel == null)
            {
                Logger.LogError("Não foi possível iniciar consumidor pois o canal não foi instanciado");
                return;
            }

            var consumer = new AsyncEventingBasicConsumer(ConsumerChannel);

            consumer.Received += OnMessageReceived;

            ConsumerChannel.BasicConsume(
                queue: QueueName,
                autoAck: false,
                consumer: consumer);
        }

        private async Task OnMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            try
            {
                await ProcessEvent(eventName, message);
                ConsumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "----- Erro ao processar mensagem \"{Message}\"", message);
                ConsumerChannel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            Logger.LogTrace("Processando evento do RabbitMQ: {EventName}", eventName);

            if (!SubscriptionManager.HasSubscriptionsForEvent(eventName))
            {
                Logger.LogWarning("Não existem consumidores para o evendo do RabbitMQ: {EventName}", eventName);
                return;
            }

            var subscriptions = SubscriptionManager.GetHandlersForEvent(eventName);    
            foreach (var subscription in subscriptions)
            {
                var handler = Service­Provider.GetService(subscription.HandlerType);
                var method = subscription.HandlerType.GetMethod("Handle");
                var eventType = SubscriptionManager.GetEventTypeByName(eventName);
                var integrationEvent = (IntegrationEvent)JsonConvert.DeserializeObject(message, eventType);

                //Forcing asynchronous call
                await Task.Yield();
                await (Task) method.Invoke(handler, new object[] { integrationEvent });
            }
        }
    
        private AsyncRetryPolicy CreateRetryPolicy(IntegrationEvent @event)
        {
            return RetryPolicy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetryAsync(RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    Logger.LogWarning(ex, "Não foi possível publicar o evento: {EventId} após {Timeout}s ({ExceptionMessage})", 
                        @event.Id, $"{time.TotalSeconds:n1}", ex.Message);
                });
        }

        private IModel CreateConsumerChannel()
        {
            ConnectToBroker();
            Logger.LogTrace("Criando canal para o consumidor no RabbitMQ");
            var channel = PersistentConnection.CreateModel();
            CreateExchange(channel);
            CreateQueue(channel);
            SettingExceptionCallBack(channel);
            return channel;
        }

        private void SettingExceptionCallBack(IModel channel)
        {
            channel.CallbackException += (sender, ea) =>
            {
                Logger.LogWarning(ea.Exception, "Recriando canal para o consumidor no RabbitMQ");
                ConsumerChannel.Dispose();
                ConsumerChannel = CreateConsumerChannel();
                StartBasicConsume();
            };
        }

        private void CreateExchange(IModel channel)
        {
            channel.ExchangeDeclare(CENSUS_EXCHANGE, ExchangeType.Fanout);
        }

        private void CreateQueue(IModel channel)
        {
            var args = new Dictionary<string, object>
            {
                { "x-message-ttl", 10000 }
            };

            channel.QueueDeclare(queue: QueueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: args);


            channel.QueueBind(QueueName, CENSUS_EXCHANGE, string.Empty);
        }

        private void ConnectToBroker()
        {
            if (!PersistentConnection.IsConnected)
            {
                PersistentConnection.TryConnect();
            }
        }

    }
}
