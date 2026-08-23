using System.Threading.Tasks;
using Census.FamilyTree.Domain.Entities;

namespace Census.FamilyTree.Domain.Repository
{
    public interface IPersonFamilyTreeRepository
    {
        Task<PersonFamilyTree> GetFamilyTree(string personId, uint level);

        Task AddNode(PersonFamilyTreeNode personFamilyTreeNode);

        Task UpdateNode(PersonFamilyTreeNode oldNode, PersonFamilyTreeNode newNode);

        Task RemoveNode(PersonFamilyTreeNode personFamilyTreeNode);
    }
}
