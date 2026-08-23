using Census.Shared.Bus.Interfaces;
using Census.People.Infra.Connection;
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

            var indexKeys = Builders<OutboxMessageDocument>.IndexKeys.Ascending(x => x.PublishedAt);
            _collection.Indexes.CreateOne(new CreateIndexModel<OutboxMessageDocument>(indexKeys));
        }

        public async Task SaveAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            var document = OutboxMessageDocument.From(message);
            await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        }

        public async Task<IReadOnlyList<OutboxMessage>> GetUnpublishedAsync(int batchSize, CancellationToken cancellationToken = default)
        {
            var filter = Builders<OutboxMessageDocument>.Filter.Eq(x => x.PublishedAt, null);
            var documents = await _collection.Find(filter)
                .SortBy(x => x.CreatedAt)
                .Limit(batchSize)
                .ToListAsync(cancellationToken);

            return documents.Select(document => document.ToMessage()).ToList();
        }

        public async Task MarkAsPublishedAsync(Guid messageId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<OutboxMessageDocument>.Filter.Eq(x => x.Id, messageId);
            var update = Builders<OutboxMessageDocument>.Update.Set(x => x.PublishedAt, DateTime.UtcNow);
            await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }

        private class OutboxMessageDocument
        {
            public Guid Id { get; set; }

            public string EventType { get; set; } = string.Empty;

            public string Payload { get; set; } = string.Empty;

            public DateTime CreatedAt { get; set; }

            public DateTime? PublishedAt { get; set; }

            public string? CorrelationId { get; set; }

            public static OutboxMessageDocument From(OutboxMessage message) =>
                new()
                {
                    Id = message.Id,
                    EventType = message.EventType,
                    Payload = message.Payload,
                    CreatedAt = message.CreatedAt,
                    PublishedAt = message.PublishedAt,
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
                    CorrelationId = CorrelationId
                };
        }
    }
}
