using System;
using System.Collections.Generic;
using System.Text;

namespace Census.FamilyTree.Domain.Entities
{
    public class PersonFamilyTreeRelationship
    {
        public string ParentId { get; set; }

        public string ChildId { get; set; }
    }
}
