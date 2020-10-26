using Census.FamilyTree.Domain.Entities;
using MediatR;

namespace Census.FamilyTree.Application.Queries
{
    public class FamilyTreeQuery : IRequest<PersonFamilyTree>
    {
        public string PersonId { get; set; }

        public uint Level { get; set; }
    }
}
