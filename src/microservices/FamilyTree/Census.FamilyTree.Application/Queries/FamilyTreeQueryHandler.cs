using Census.FamilyTree.Domain.Entities;
using Census.FamilyTree.Domain.Repository;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Census.FamilyTree.Application.Queries
{
    public class FamilyTreeQueryHandler : IRequestHandler<FamilyTreeQuery, PersonFamilyTree>
    {
        private readonly IPersonFamilyTreeRepository _personFamilyTreeRepository;

        public FamilyTreeQueryHandler(IPersonFamilyTreeRepository personFamilyTreeRepository)
        {
            _personFamilyTreeRepository = personFamilyTreeRepository;
        }

        public Task<PersonFamilyTree> Handle(FamilyTreeQuery request, CancellationToken cancellationToken)
        {
            // Neo4j is updated only via RabbitMQ integration events (eventual consistency).
            return _personFamilyTreeRepository.GetFamilyTree(request.PersonId, request.Level);
        }
    }
}
