using Microsoft.Extensions.Configuration;
using Neo4j.Driver;
using Neo4jClient;
using System;
using System.Threading.Tasks;

namespace Census.FamilyTree.Infra.Connection
{
    public class Neo4jConnection : INeo4jConnection
    {
        public GraphClient GraphClient { get; set; }

        public Neo4jConnection(IConfiguration configuration)
        {
            var neo4jConfig = configuration.GetSection("Neo4j");
            var uri = neo4jConfig["Uri"];
            var userName = neo4jConfig["Username"];
            var password = neo4jConfig["Password"];
            GraphClient = new GraphClient(new Uri(uri), userName, password);
            
        }

        public async Task<GraphClient> GetClient()
        {
            if (!GraphClient.IsConnected)
                await GraphClient.ConnectAsync();

            return GraphClient;
        }
    }
}
