using System.Threading.Tasks;
using Neo4jClient;

namespace Census.FamilyTree.Infra.Connection
{
    public interface INeo4jConnection
    {
        Task<IGraphClient> GetClient();
    }
}
