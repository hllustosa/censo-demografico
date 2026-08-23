using System;
using System.Threading;
using System.Threading.Tasks;

namespace Census.Shared.Bus.Interfaces
{
    public interface IOutboxStore
    {
        Task SaveAsync(OutboxMessage message, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<OutboxMessage>> GetUnpublishedAsync(int batchSize, CancellationToken cancellationToken = default);

        Task MarkAsPublishedAsync(Guid messageId, CancellationToken cancellationToken = default);
    }

    public class OutboxMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string EventType { get; set; } = string.Empty;

        public string Payload { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PublishedAt { get; set; }

        public string? CorrelationId { get; set; }
    }
}
