using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Census.FamilyTree.Domain.Entities;
using Census.FamilyTree.Domain.Repository;
using Census.FamilyTree.Infra.Connection;
using Census.FamilyTree.Infra.Repository;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Census.FamilyTree.Test.Integration
{
    [Collection("Neo4jIntegration")]
    public class TestGetFamilyTreeRepository
    {
        private readonly IPersonFamilyTreeRepository _repository;
        private readonly INeo4jConnection _connection;

        public TestGetFamilyTreeRepository(Neo4jFixture fixture)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddInMemoryCollection(fixture.ConfigurationOverrides())
                .Build();

            _connection = new Neo4jConnection(configuration);
            _repository = new PersonFamilyTreeRepository(_connection);
        }

        [Fact]
        public async Task GetFamilyTree_ReturnsRootWhenPersonHasNoRelatives()
        {
            await ClearGraph();
            await _repository.AddNode(new PersonFamilyTreeNode { Id = "solo", Name = "Solo" });

            var tree = await _repository.GetFamilyTree("solo", 2);

            Assert.Single(tree.Nodes);
            Assert.Equal("solo", tree.Nodes[0].Id);
            Assert.Empty(tree.Relationships);
        }

        [Fact]
        public async Task GetFamilyTree_ReturnsAncestorsAndDescendantsForMiddlePerson()
        {
            await SeedThreeGenerationFamily();

            var tree = await _repository.GetFamilyTree("3", 2);

            Assert.Contains(tree.Nodes, node => node.Id == "1");
            Assert.Contains(tree.Nodes, node => node.Id == "2");
            Assert.Contains(tree.Nodes, node => node.Id == "3");
            Assert.Contains(tree.Nodes, node => node.Id == "4");
            Assert.Contains(tree.Nodes, node => node.Id == "5");
            Assert.Contains(tree.Relationships, rel => rel.ParentId == "1" && rel.ChildId == "3");
            Assert.Contains(tree.Relationships, rel => rel.ParentId == "2" && rel.ChildId == "3");
            Assert.Contains(tree.Relationships, rel => rel.ParentId == "3" && rel.ChildId == "5");
            Assert.Contains(tree.Relationships, rel => rel.ParentId == "4" && rel.ChildId == "5");
        }

        [Fact]
        public async Task GetFamilyTree_RespectsLevelAndExcludesFartherGenerations()
        {
            await SeedFourGenerationFamily();

            var levelOne = await _repository.GetFamilyTree("child", 1);
            var levelTwo = await _repository.GetFamilyTree("child", 2);

            Assert.Contains(levelOne.Nodes, node => node.Id == "child");
            Assert.Contains(levelOne.Nodes, node => node.Id == "parent");
            Assert.Contains(levelOne.Nodes, node => node.Id == "grandchild");
            Assert.DoesNotContain(levelOne.Nodes, node => node.Id == "grandparent");
            Assert.DoesNotContain(levelOne.Nodes, node => node.Id == "greatgrandchild");

            Assert.Contains(levelTwo.Nodes, node => node.Id == "grandparent");
            Assert.Contains(levelTwo.Nodes, node => node.Id == "greatgrandchild");
            Assert.True(levelTwo.Nodes.Count > levelOne.Nodes.Count);
        }

        [Fact]
        public async Task GetFamilyTree_LinksChildCreatedBeforeParents()
        {
            await ClearGraph();

            await _repository.AddNode(new PersonFamilyTreeNode
            {
                Id = "child",
                Name = "Filho",
                FatherId = "father",
                MotherId = "mother",
            });

            await _repository.AddNode(new PersonFamilyTreeNode { Id = "father", Name = "Pai" });
            await _repository.AddNode(new PersonFamilyTreeNode { Id = "mother", Name = "Mae" });

            var tree = await _repository.GetFamilyTree("child", 1);

            Assert.Equal(3, tree.Nodes.Count);
            Assert.Contains(tree.Nodes, node => node.Id == "father");
            Assert.Contains(tree.Nodes, node => node.Id == "mother");
            Assert.Equal(2, tree.Relationships.Count);
        }

        [Fact]
        public async Task GetFamilyTree_ReturnsEmptyWhenPersonDoesNotExist()
        {
            await ClearGraph();

            var tree = await _repository.GetFamilyTree("missing", 2);

            Assert.Empty(tree.Nodes);
            Assert.Empty(tree.Relationships);
        }

        private async Task SeedThreeGenerationFamily()
        {
            await ClearGraph();

            await _repository.AddNode(new PersonFamilyTreeNode { Id = "1", Name = "Avó materna" });
            await _repository.AddNode(new PersonFamilyTreeNode { Id = "2", Name = "Avô paterno" });
            await _repository.AddNode(new PersonFamilyTreeNode
            {
                Id = "3",
                Name = "Vera",
                MotherId = "1",
                FatherId = "2",
            });
            await _repository.AddNode(new PersonFamilyTreeNode { Id = "4", Name = "Hermano" });
            await _repository.AddNode(new PersonFamilyTreeNode
            {
                Id = "5",
                Name = "Lourenço",
                MotherId = "3",
                FatherId = "4",
            });
        }

        private async Task SeedFourGenerationFamily()
        {
            await ClearGraph();

            await _repository.AddNode(new PersonFamilyTreeNode { Id = "grandparent", Name = "Avô" });
            await _repository.AddNode(new PersonFamilyTreeNode
            {
                Id = "parent",
                Name = "Pai",
                FatherId = "grandparent",
            });
            await _repository.AddNode(new PersonFamilyTreeNode
            {
                Id = "child",
                Name = "Filho",
                FatherId = "parent",
            });
            await _repository.AddNode(new PersonFamilyTreeNode
            {
                Id = "grandchild",
                Name = "Neto",
                FatherId = "child",
            });
            await _repository.AddNode(new PersonFamilyTreeNode
            {
                Id = "greatgrandchild",
                Name = "Bisneto",
                FatherId = "grandchild",
            });
        }

        private async Task ClearGraph()
        {
            var client = await _connection.GetClient();
            await client.Cypher.Match("(n:Person)").DetachDelete("n").ExecuteWithoutResultsAsync();
        }
    }
}
