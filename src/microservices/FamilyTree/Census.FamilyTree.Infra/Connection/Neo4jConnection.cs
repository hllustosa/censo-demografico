using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Neo4jClient;

namespace Census.FamilyTree.Infra.Connection
{
    public class Neo4jConnection : INeo4jConnection
    {
        private readonly BoltGraphClient _graphClient;

        public Neo4jConnection(IConfiguration configuration)
        {
            var neo4jConfig = configuration.GetSection("Neo4j");
            var uri = neo4jConfig["Uri"] ?? "bolt://localhost:7687";
            var userName = neo4jConfig["Username"] ?? "neo4j";
            var password = neo4jConfig["Password"] ?? throw new InvalidOperationException("Neo4j:Password is required.");
            _graphClient = new BoltGraphClient(new Uri(uri), userName, password);
        }

        public async Task<IGraphClient> GetClient()
        {
            if (!_graphClient.IsConnected)
            {
                await _graphClient.ConnectAsync();
            }

            return _graphClient;
        }
    }
}
