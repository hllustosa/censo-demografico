namespace Census.FamilyTree.Domain.Entities
{
    public class PersonFamilyTreeNode
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string FatherId { get; set; }

        public string MotherId { get; set; }
    }
}
