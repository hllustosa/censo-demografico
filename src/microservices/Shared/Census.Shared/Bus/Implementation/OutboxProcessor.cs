using Census.Shared.Bus.Interfaces;
using Census.Shared.Observability;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Census.Shared.Bus.Implementation
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxProcessor> _logger;

        public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var outboxStore = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
                    var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

                    var messages = await outboxStore.GetUnpublishedAsync(20, stoppingToken);
                    foreach (var message in messages)
                    {
                        var eventType = Type.GetType(message.EventType);
                        if (eventType == null)
                        {
                            _logger.LogWarning("Unknown outbox event type {EventType}", message.EventType);
                            continue;
                        }

                        var integrationEvent = (IntegrationEvent)JsonConvert.DeserializeObject(message.Payload, eventType)!;
                        integrationEvent.CorrelationId ??= message.CorrelationId;

                        if (!string.IsNullOrEmpty(integrationEvent.CorrelationId))
                        {
                            CorrelationContext.CorrelationId = integrationEvent.CorrelationId;
                        }

                        await eventBus.PublishAsync(integrationEvent, stoppingToken);
                        await outboxStore.MarkAsPublishedAsync(message.Id, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Outbox processor iteration failed");
                }

                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }
}
