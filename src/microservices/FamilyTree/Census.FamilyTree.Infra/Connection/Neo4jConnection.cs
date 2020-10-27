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

        private Neo4jConnection(GraphClient graphClient)
        {
            GraphClient = graphClient;
        }

        public static Neo4jConnection Create()
        {
            var uri = "http://neo4j:7474/db/data";
            var userName = "neo4j";
            var password = "test";
            var graphClient = new GraphClient(new Uri(uri), userName, password);

            return new Neo4jConnection(graphClient);
        }

        public async Task<GraphClient> GetClient()
        {
            if (!GraphClient.IsConnected)
                await GraphClient.ConnectAsync();

            return GraphClient;
        }
    }
}
