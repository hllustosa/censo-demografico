using System.Threading;
using System.Threading.Tasks;

namespace Census.FamilyTree.Application.Services
{
    public interface IPersonGraphSyncService
    {
        Task SyncPersonSubtreeAsync(string personId, uint level, CancellationToken cancellationToken = default);
    }
}
