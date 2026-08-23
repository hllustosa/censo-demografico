using Census.People.Domain.Interfaces;
using Census.People.Infra.Connection;
using Census.People.Infra.Repository;
using Census.Shared.Bus.Interfaces;
using MongoDB.Driver;

namespace Census.People.Infra.Outbox
{
    public class MongoOutboxStore : IOutboxStore
    {
        private const string CollectionName = "outbox";

        private readonly IMongoCollection<OutboxMessageDocument> _collection;

        public MongoOutboxStore(IMongoConnection mongoConnection)
        {
            _collection = mongoConnection.GetDatabase()
                .GetCollection<OutboxMessageDocument>(CollectionName);

            var publishedIndex = Builders<OutboxMessageDocument>.IndexKeys
                .Ascending(x => x.PublishedAt)
                .Ascending(x => x.FailedAt)
                .Ascending(x => x.LockedUntil)
                .Ascending(x => x.CreatedAt);
            _collection.Indexes.CreateOne(new CreateIndexModel<OutboxMessageDocument>(publishedIndex));
        }

        public Task SaveAsync(OutboxMessage message, CancellationToken cancellationToken = default) =>
            SaveInternalAsync(message, session: null, cancellationToken);

        public Task SaveAsync(OutboxMessage message, object transactionContext, CancellationToken cancellationToken = default)
        {
            var session = ((MongoSession)transactionContext).Session;
            return SaveInternalAsync(message, session, cancellationToken);
        }

        private async Task SaveInternalAsync(
            OutboxMessage message,
            IClientSessionHandle? session,
            CancellationToken cancellationToken)
        {
            var document = OutboxMessageDocument.From(message);
            if (session is null)
            {
                await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
                return;
            }

            await _collection.InsertOneAsync(session, document, cancellationToken: cancellationToken);
        }

        public async Task<IReadOnlyList<OutboxMessage>> ClaimUnpublishedAsync(
            int batchSize,
            string ownerId,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var claimed = new List<OutboxMessage>();

            for (var i = 0; i < batchSize; i++)
            {
                var filter = Builders<OutboxMessageDocument>.Filter.And(
                    Builders<OutboxMessageDocument>.Filter.Eq(x => x.PublishedAt, null),
                    Builders<OutboxMessageDocument>.Filter.Eq(x => x.FailedAt, null),
                    Builders<OutboxMessageDocument>.Filter.Or(
                        Builders<OutboxMessageDocument>.Filter.Eq(x => x.LockedUntil, null),
                        Builders<OutboxMessageDocument>.Filter.Lt(x => x.LockedUntil, now)));

                var update = Builders<OutboxMessageDocument>.Update
                    .Set(x => x.LockedUntil, now.Add(leaseDuration))
                    .Set(x => x.LockedBy, ownerId);

                var options = new FindOneAndUpdateOptions<OutboxMessageDocument>
                {
                    Sort = Builders<OutboxMessageDocument>.Sort.Ascending(x => x.CreatedAt),
                    ReturnDocument = ReturnDocument.After
                };

                var document = await _collection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
                if (document is null)
                {
                    break;
                }

                claimed.Add(document.ToMessage());
            }

            return claimed;
        }

        public async Task MarkAsPublishedAsync(Guid messageId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<OutboxMessageDocument>.Filter.Eq(x => x.Id, messageId);
            var update = Builders<OutboxMessageDocument>.Update
                .Set(x => x.PublishedAt, DateTime.UtcNow)
                .Unset(x => x.LockedUntil)
                .Unset(x => x.LockedBy);
            await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }

        public async Task MarkAsFailedAsync(Guid messageId, string reason, CancellationToken cancellationToken = default)
        {
            var filter = Builders<OutboxMessageDocument>.Filter.Eq(x => x.Id, messageId);
            var update = Builders<OutboxMessageDocument>.Update
                .Set(x => x.FailedAt, DateTime.UtcNow)
                .Set(x => x.FailureReason, reason)
                .Unset(x => x.LockedUntil)
                .Unset(x => x.LockedBy);
            await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }

        public async Task<long> CountPendingAsync(CancellationToken cancellationToken = default)
        {
            var filter = Builders<OutboxMessageDocument>.Filter.And(
                Builders<OutboxMessageDocument>.Filter.Eq(x => x.PublishedAt, null),
                Builders<OutboxMessageDocument>.Filter.Eq(x => x.FailedAt, null));
            return await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        }

        private class OutboxMessageDocument
        {
            public Guid Id { get; set; }

            public string EventType { get; set; } = string.Empty;

            public string Payload { get; set; } = string.Empty;

            public DateTime CreatedAt { get; set; }

            public DateTime? PublishedAt { get; set; }

            public DateTime? FailedAt { get; set; }

            public string? FailureReason { get; set; }

            public DateTime? LockedUntil { get; set; }

            public string? LockedBy { get; set; }

            public string? CorrelationId { get; set; }

            public static OutboxMessageDocument From(OutboxMessage message) =>
                new()
                {
                    Id = message.Id,
                    EventType = message.EventType,
                    Payload = message.Payload,
                    CreatedAt = message.CreatedAt,
                    PublishedAt = message.PublishedAt,
                    FailedAt = message.FailedAt,
                    FailureReason = message.FailureReason,
                    LockedUntil = message.LockedUntil,
                    LockedBy = message.LockedBy,
                    CorrelationId = message.CorrelationId
                };

            public OutboxMessage ToMessage() =>
                new()
                {
                    Id = Id,
                    EventType = EventType,
                    Payload = Payload,
                    CreatedAt = CreatedAt,
                    PublishedAt = PublishedAt,
                    FailedAt = FailedAt,
                    FailureReason = FailureReason,
                    LockedUntil = LockedUntil,
                    LockedBy = LockedBy,
                    CorrelationId = CorrelationId
                };
        }
    }
}
