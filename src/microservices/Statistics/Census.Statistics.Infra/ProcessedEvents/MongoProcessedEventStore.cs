using Census.Shared.Bus.Interfaces;
using Census.Statistics.Infra.Connection;
using MongoDB.Driver;

namespace Census.Statistics.Infra.ProcessedEvents
{
    public class MongoProcessedEventStore : IProcessedEventStore
    {
        private const string CollectionName = "processed_events";

        private readonly IMongoCollection<ProcessedEventDocument> _collection;

        public MongoProcessedEventStore(IMongoConnection mongoConnection)
        {
            _collection = mongoConnection.GetDatabase()
                .GetCollection<ProcessedEventDocument>(CollectionName);

            var indexKeys = Builders<ProcessedEventDocument>.IndexKeys.Ascending(x => x.EventId);
            _collection.Indexes.CreateOne(
                new CreateIndexModel<ProcessedEventDocument>(indexKeys, new CreateIndexOptions { Unique = true }));
        }

        public async Task<bool> HasBeenProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<ProcessedEventDocument>.Filter.Eq(x => x.EventId, eventId);
            return await _collection.Find(filter).AnyAsync(cancellationToken);
        }

        public async Task MarkAsProcessedAsync(Guid eventId, string eventType, CancellationToken cancellationToken = default)
        {
            var document = new ProcessedEventDocument
            {
                EventId = eventId,
                EventType = eventType,
                ProcessedAt = DateTime.UtcNow
            };

            await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        }

        private class ProcessedEventDocument
        {
            public Guid EventId { get; set; }

            public string EventType { get; set; } = string.Empty;

            public DateTime ProcessedAt { get; set; }
        }
    }
}
