using System;
using System.Threading;
using System.Threading.Tasks;

namespace Census.Shared.Bus.Interfaces
{
    public interface IProcessedEventStore
    {
        Task<bool> HasBeenProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);

        Task MarkAsProcessedAsync(Guid eventId, string eventType, CancellationToken cancellationToken = default);
    }
}
