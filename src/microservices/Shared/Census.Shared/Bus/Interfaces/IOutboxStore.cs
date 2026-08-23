using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Census.Shared.Bus.Interfaces
{
    public interface IOutboxStore
    {
        Task SaveAsync(OutboxMessage message, CancellationToken cancellationToken = default);

        Task SaveAsync(OutboxMessage message, object transactionContext, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<OutboxMessage>> ClaimUnpublishedAsync(
            int batchSize,
            string ownerId,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default);

        Task MarkAsPublishedAsync(Guid messageId, CancellationToken cancellationToken = default);

        Task MarkAsFailedAsync(Guid messageId, string reason, CancellationToken cancellationToken = default);

        Task<long> CountPendingAsync(CancellationToken cancellationToken = default);
    }

    public class OutboxMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string EventType { get; set; } = string.Empty;

        public string Payload { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PublishedAt { get; set; }

        public DateTime? FailedAt { get; set; }

        public string? FailureReason { get; set; }

        public DateTime? LockedUntil { get; set; }

        public string? LockedBy { get; set; }

        public string? CorrelationId { get; set; }
    }
}
