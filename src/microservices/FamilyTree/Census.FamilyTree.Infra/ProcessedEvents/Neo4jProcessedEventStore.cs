using Census.FamilyTree.Infra.Connection;
using Census.Shared.Bus.Interfaces;
using Neo4jClient;
using Neo4jClient.Cypher;

namespace Census.FamilyTree.Infra.ProcessedEvents
{
    public class Neo4jProcessedEventStore : IProcessedEventStore
    {
        private readonly INeo4jConnection _neo4jConnection;

        public Neo4jProcessedEventStore(INeo4jConnection neo4jConnection)
        {
            _neo4jConnection = neo4jConnection;
        }

        public async Task<bool> HasBeenProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            var client = await _neo4jConnection.GetClient();
            var results = await client.Cypher
                .Match("(event:ProcessedEvent {eventId: $eventId})")
                .WithParam("eventId", eventId.ToString())
                .Return(eventNode => eventNode.Count())
                .ResultsAsync;

            return results.FirstOrDefault() > 0;
        }

        public async Task MarkAsProcessedAsync(Guid eventId, string eventType, CancellationToken cancellationToken = default)
        {
            var client = await _neo4jConnection.GetClient();
            await client.Cypher
                .Create("(event:ProcessedEvent {eventId: $eventId, eventType: $eventType, processedAt: $processedAt})")
                .WithParams(new
                {
                    eventId = eventId.ToString(),
                    eventType,
                    processedAt = DateTime.UtcNow.ToString("O")
                })
                .ExecuteWithoutResultsAsync();
        }
    }
}
