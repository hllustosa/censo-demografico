using System.Collections.Generic;

namespace Census.FamilyTree.Domain.Entities
{
    public class PersonFamilyTree
    {
        public List<PersonFamilyTreeNode> Nodes { get; set; }

        public List<PersonFamilyTreeRelationship> Relationships { get; set; }
    }
}
