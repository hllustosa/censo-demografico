using Census.FamilyTree.Application.Services;
using Census.FamilyTree.Domain.Entities;
using Census.FamilyTree.Domain.Repository;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Census.FamilyTree.Application.Queries
{
    public class FamilyTreeQueryHandler : IRequestHandler<FamilyTreeQuery, PersonFamilyTree>
    {
        IPersonFamilyTreeRepository PersonFamilyTreeRepository { get; set; }
        IPersonGraphSyncService PersonGraphSyncService { get; set; }

        public FamilyTreeQueryHandler(
            IPersonFamilyTreeRepository personFamilyTreeRepository,
            IPersonGraphSyncService personGraphSyncService)
        {
            PersonFamilyTreeRepository = personFamilyTreeRepository;
            PersonGraphSyncService = personGraphSyncService;
        }

        public async Task<PersonFamilyTree> Handle(FamilyTreeQuery request, CancellationToken cancellationToken)
        {
            try
            {
                await PersonGraphSyncService.SyncPersonSubtreeAsync(
                    request.PersonId,
                    request.Level,
                    cancellationToken);
            }
            catch
            {
                // Keep serving Neo4j data when People sync is unavailable.
            }

            return await PersonFamilyTreeRepository.GetFamilyTree(request.PersonId, request.Level);
        }
    }
}
