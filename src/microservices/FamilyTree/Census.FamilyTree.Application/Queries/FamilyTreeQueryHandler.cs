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

        public FamilyTreeQueryHandler(IPersonFamilyTreeRepository personFamilyTreeRepository)
        {
            PersonFamilyTreeRepository = personFamilyTreeRepository;
        }

        public Task<PersonFamilyTree> Handle(FamilyTreeQuery request, CancellationToken cancellationToken)
        {
            return PersonFamilyTreeRepository.GetFamilyTree(request.PersonId, request.Level);
        }
    }
}
