using Neo4jClient;
using System.Threading.Tasks;

namespace Census.FamilyTree.Infra.Connection
{
    public interface INeo4jConnection
    {
        Task<GraphClient> GetClient();
    }
}
