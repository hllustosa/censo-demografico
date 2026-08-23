using System.Net.Sockets;
using System.Text;
using Census.Shared.Bus.Interfaces;
using Census.Shared.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Census.Shared.Bus.Implementation
{
    public class RabbitMQEventBus : IEventBus
    {
        private const byte PersistentMode = 2;
        private const string CensusExchange = "census";
        private const string DeadLetterExchange = "census.dlx";
        private const string RetryHeader = "x-retry-count";
        private const string CorrelationHeader = "x-correlation-id";
        private const int MaxConsumeRetries = 3;

        private readonly IPersistentConnection _persistentConnection;
        private readonly IEventBusSubscriptionsManager _subscriptionManager;
        private readonly ILogger<RabbitMQEventBus> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly EventBusMetrics _metrics;
        private readonly string? _queueName;
        private readonly bool _publisherOnly;
        private readonly int _retryCount;

        private IModel? _consumerChannel;
        private bool _consumerStarted;

        public RabbitMQEventBus(
            IPersistentConnection persistentConnection,
            IEventBusSubscriptionsManager subscriptionManager,
            ILogger<RabbitMQEventBus> logger,
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            EventBusMetrics metrics)
        {
            var rabbitMqConfig = configuration.GetSection("RabbitMqConnection");
            _queueName = rabbitMqConfig["QueueName"];
            _publisherOnly = bool.TryParse(rabbitMqConfig["PublisherOnly"], out var publisherOnly) && publisherOnly;
            _retryCount = int.Parse(rabbitMqConfig["retryCount"] ?? "5");

            _persistentConnection = persistentConnection;
            _subscriptionManager = subscriptionManager;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _metrics = metrics;

            EnsureExchangeExists();
        }

        public async Task PublishAsync(IntegrationEvent @event, CancellationToken cancellationToken = default)
        {
            ConnectToBroker();
            var policy = CreatePublishRetryPolicy(@event);
            var eventName = @event.GetType().Name;

            if (string.IsNullOrEmpty(@event.CorrelationId))
            {
                @event.CorrelationId = CorrelationContext.EnsureCorrelationId();
            }

            using var channel = _persistentConnection.CreateModel();
            EnsureExchangeExists(channel);

            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            await policy.ExecuteAsync(_ =>
            {
                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = PersistentMode;
                properties.Headers = new Dictionary<string, object>
                {
                    [CorrelationHeader] = @event.CorrelationId ?? string.Empty
                };

                _logger.LogInformation("Publishing event {EventName} with id {EventId}", eventName, @event.Id);
                channel.BasicPublish(
                    exchange: CensusExchange,
                    routingKey: eventName,
                    mandatory: false,
                    basicProperties: properties,
                    body: body);

                _metrics.RecordPublished(eventName);
                return Task.CompletedTask;
            }, cancellationToken);
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            if (_publisherOnly)
            {
                throw new InvalidOperationException("Cannot subscribe on a publisher-only event bus instance.");
            }

            _subscriptionManager.AddSubscription<T, TH>();
            StartBasicConsume();
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subscriptionManager.GetEventKey<T>();
            _logger.LogInformation("Unsubscribing from event {EventName}", eventName);
            _subscriptionManager.RemoveSubscription<T, TH>();
        }

        private void EnsureExchangeExists(IModel? channel = null)
        {
            ConnectToBroker();
            var ownsChannel = channel == null;
            channel ??= _persistentConnection.CreateModel();

            try
            {
                channel.ExchangeDeclare(CensusExchange, ExchangeType.Fanout, durable: true);
                channel.ExchangeDeclare(DeadLetterExchange, ExchangeType.Fanout, durable: true);
            }
            finally
            {
                if (ownsChannel)
                {
                    channel.Dispose();
                }
            }
        }

        private void StartBasicConsume()
        {
            if (_consumerStarted || _publisherOnly || string.IsNullOrWhiteSpace(_queueName))
            {
                return;
            }

            _consumerChannel = CreateConsumerChannel();
            var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
            consumer.Received += OnMessageReceived;

            _consumerChannel.BasicConsume(
                queue: _queueName,
                autoAck: false,
                consumer: consumer);

            _consumerStarted = true;
            _logger.LogInformation("Started consuming queue {QueueName}", _queueName);
        }

        private async Task OnMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            if (_consumerChannel == null)
            {
                return;
            }

            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            var retryCount = GetRetryCount(eventArgs.BasicProperties);
            var correlationId = GetCorrelationId(eventArgs.BasicProperties);

            if (!string.IsNullOrEmpty(correlationId))
            {
                CorrelationContext.CorrelationId = correlationId;
            }

            try
            {
                await ProcessEventAsync(eventName, message);
                _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                _metrics.RecordConsumed(eventName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process message for event {EventName}", eventName);
                _metrics.RecordFailed(eventName);

                if (retryCount >= MaxConsumeRetries)
                {
                    PublishToDeadLetter(eventArgs, eventName, retryCount);
                    _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                    _metrics.RecordDeadLettered(eventName);
                    return;
                }

                PublishToRetry(eventArgs, eventName, retryCount + 1);
                _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                _metrics.RecordRetried(eventName);
            }
        }

        private async Task ProcessEventAsync(string eventName, string message)
        {
            if (!_subscriptionManager.HasSubscriptionsForEvent(eventName))
            {
                _logger.LogWarning("No handlers registered for event {EventName}", eventName);
                return;
            }

            var subscriptions = _subscriptionManager.GetHandlersForEvent(eventName);
            var eventType = _subscriptionManager.GetEventTypeByName(eventName);
            var integrationEvent = (IntegrationEvent)JsonConvert.DeserializeObject(message, eventType)!;

            using var scope = _serviceProvider.CreateScope();

            foreach (var subscription in subscriptions)
            {
                var handler = scope.ServiceProvider.GetService(subscription.HandlerType);
                if (handler == null)
                {
                    continue;
                }

                var processedEventStore = scope.ServiceProvider.GetService<IProcessedEventStore>();
                if (processedEventStore != null &&
                    await processedEventStore.HasBeenProcessedAsync(integrationEvent.Id))
                {
                    _logger.LogInformation(
                        "Skipping duplicate event {EventId} of type {EventName}",
                        integrationEvent.Id,
                        eventName);
                    _metrics.RecordDuplicate(eventName);
                    continue;
                }

                var method = subscription.HandlerType.GetMethod("Handle");
                await (Task)method!.Invoke(handler, new object[] { integrationEvent })!;

                if (processedEventStore != null)
                {
                    await processedEventStore.MarkAsProcessedAsync(integrationEvent.Id, eventName);
                }
            }
        }

        private void PublishToRetry(BasicDeliverEventArgs eventArgs, string eventName, int retryCount)
        {
            using var channel = _persistentConnection.CreateModel();
            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = PersistentMode;
            properties.Headers = new Dictionary<string, object>
            {
                [RetryHeader] = retryCount
            };

            if (eventArgs.BasicProperties.Headers?.TryGetValue(CorrelationHeader, out var correlation) == true)
            {
                properties.Headers[CorrelationHeader] = correlation;
            }

            channel.BasicPublish(
                exchange: CensusExchange,
                routingKey: eventName,
                mandatory: false,
                basicProperties: properties,
                body: eventArgs.Body.ToArray());
        }

        private void PublishToDeadLetter(BasicDeliverEventArgs eventArgs, string eventName, int retryCount)
        {
            using var channel = _persistentConnection.CreateModel();
            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = PersistentMode;
            properties.Headers = new Dictionary<string, object>
            {
                [RetryHeader] = retryCount,
                ["x-original-event"] = eventName
            };

            channel.BasicPublish(
                exchange: DeadLetterExchange,
                routingKey: eventName,
                mandatory: false,
                basicProperties: properties,
                body: eventArgs.Body.ToArray());

            _logger.LogError(
                "Message for event {EventName} moved to dead-letter exchange after {RetryCount} retries",
                eventName,
                retryCount);
        }

        private static int GetRetryCount(IBasicProperties properties)
        {
            if (properties.Headers != null &&
                properties.Headers.TryGetValue(RetryHeader, out var value))
            {
                return Convert.ToInt32(value);
            }

            return 0;
        }

        private static string? GetCorrelationId(IBasicProperties properties)
        {
            if (properties.Headers != null &&
                properties.Headers.TryGetValue(CorrelationHeader, out var value))
            {
                return Encoding.UTF8.GetString((byte[])value);
            }

            return null;
        }

        private AsyncRetryPolicy CreatePublishRetryPolicy(IntegrationEvent @event)
        {
            return Policy
                .Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetryAsync(
                    _retryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) =>
                    {
                        _logger.LogWarning(
                            ex,
                            "Could not publish event {EventId} after {Timeout}s",
                            @event.Id,
                            time.TotalSeconds);
                    });
        }

        private IModel CreateConsumerChannel()
        {
            ConnectToBroker();
            var channel = _persistentConnection.CreateModel();
            EnsureExchangeExists(channel);
            DeclareConsumerQueue(channel);
            channel.CallbackException += (_, ea) =>
            {
                _logger.LogWarning(ea.Exception, "Recreating consumer channel");
                _consumerChannel?.Dispose();
                _consumerChannel = CreateConsumerChannel();
                StartBasicConsume();
            };

            return channel;
        }

        private void DeclareConsumerQueue(IModel channel)
        {
            var deadLetterQueue = $"{_queueName}.dlq";
            channel.QueueDeclare(deadLetterQueue, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(deadLetterQueue, DeadLetterExchange, string.Empty);

            channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.QueueBind(_queueName!, CensusExchange, string.Empty);
        }

        private void ConnectToBroker()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }
        }
    }
}
