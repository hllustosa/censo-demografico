using Census.Shared.Bus;
using Census.Shared.Bus.Interfaces;
using Census.Shared.Observability;
using Newtonsoft.Json;

namespace Census.People.Application.Services
{
    public interface IIntegrationEventPublisher
    {
        Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
    }

    public class OutboxIntegrationEventPublisher : IIntegrationEventPublisher
    {
        private readonly IOutboxStore _outboxStore;

        public OutboxIntegrationEventPublisher(IOutboxStore outboxStore)
        {
            _outboxStore = outboxStore;
        }

        public Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        {
            integrationEvent.CorrelationId ??= CorrelationContext.EnsureCorrelationId();

            var message = new OutboxMessage
            {
                EventType = integrationEvent.GetType().AssemblyQualifiedName!,
                Payload = JsonConvert.SerializeObject(integrationEvent),
                CorrelationId = integrationEvent.CorrelationId
            };

            return _outboxStore.SaveAsync(message, cancellationToken);
        }
    }
}
