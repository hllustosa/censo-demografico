using Census.Shared.Bus.Interfaces;
using Census.Shared.Observability;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;

namespace Census.Shared.Bus.Implementation
{
    public class OutboxProcessor : BackgroundService
    {
        private static readonly Meter Meter = new("Census.Outbox");
        private static readonly Counter<long> PublishedCounter = Meter.CreateCounter<long>("outbox_messages_published_total");
        private static readonly Counter<long> FailedCounter = Meter.CreateCounter<long>("outbox_messages_failed_total");

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxProcessor> _logger;
        private readonly string _ownerId = $"{Environment.MachineName}-{Guid.NewGuid():N}";

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

                    var messages = await outboxStore.ClaimUnpublishedAsync(
                        batchSize: 20,
                        ownerId: _ownerId,
                        leaseDuration: TimeSpan.FromSeconds(30),
                        stoppingToken);

                    foreach (var message in messages)
                    {
                        var eventType = Type.GetType(message.EventType);
                        if (eventType == null)
                        {
                            _logger.LogError(
                                "Poison outbox message {MessageId}: unknown event type {EventType}",
                                message.Id,
                                message.EventType);
                            await outboxStore.MarkAsFailedAsync(
                                message.Id,
                                $"Unknown event type: {message.EventType}",
                                stoppingToken);
                            FailedCounter.Add(1);
                            continue;
                        }

                        var integrationEvent = (IntegrationEvent)JsonConvert.DeserializeObject(message.Payload, eventType)!;
                        integrationEvent.CorrelationId ??= message.CorrelationId;

                        if (!string.IsNullOrEmpty(integrationEvent.CorrelationId))
                        {
                            CorrelationContext.CorrelationId = integrationEvent.CorrelationId;
                        }

                        try
                        {
                            await eventBus.PublishAsync(integrationEvent, stoppingToken);
                            await outboxStore.MarkAsPublishedAsync(message.Id, stoppingToken);
                            PublishedCounter.Add(1);
                        }
                        catch (Exception publishEx)
                        {
                            // Leave lease to expire so another iteration can retry.
                            _logger.LogWarning(
                                publishEx,
                                "Failed to publish outbox message {MessageId}; will retry after lease expires",
                                message.Id);
                            FailedCounter.Add(1);
                        }
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
