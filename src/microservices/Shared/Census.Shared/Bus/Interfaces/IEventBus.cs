using System.Threading;
using System.Threading.Tasks;

namespace Census.Shared.Bus.Interfaces
{
    public interface IEventBus
    {
        Task PublishAsync(IntegrationEvent @event, CancellationToken cancellationToken = default);

        void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        void Unsubscribe<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent;
    }
}
