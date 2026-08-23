using Census.FamilyTree.Domain.Entities;
using Census.FamilyTree.Domain.Repository;
using Census.FamilyTree.Infra.Connection;
using Neo4jClient;
using Neo4jClient.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Census.FamilyTree.Infra.Repository
{
    public class PersonFamilyTreeRepository : IPersonFamilyTreeRepository
    {
        public INeo4jConnection Neo4JConnection { get; set; }

        public PersonFamilyTreeRepository(INeo4jConnection neo4JConnection)
        {
            Neo4JConnection = neo4JConnection;
        }

        public async Task<PersonFamilyTree> GetFamilyTree(string personId, uint level)
        {
            var client = await Neo4JConnection.GetClient();
            var root = await GetNodeById(client, personId);
            if (root == null)
            {
                return EmptyTree();
            }

            await EnsureRelationshipsFromProperties(client, root);

            var nodes = await CollectNodesWithinLevel(client, root, level);
            var nodeList = nodes.Values.ToList();
            return new PersonFamilyTree
            {
                Nodes = nodeList,
                Relationships = CreateRelationshipsList(nodes, nodeList)
            };
        }

        public Task AddNode(PersonFamilyTreeNode personFamilyTreeNode) =>
            UpsertPerson(personFamilyTreeNode);

        public async Task UpdateNode(PersonFamilyTreeNode oldNode, PersonFamilyTreeNode newNode)
        {
            var client = await Neo4JConnection.GetClient();
            var txClient = (ITransactionalGraphClient)client;
            using var transaction = txClient.BeginTransaction();

            try
            {
                if (!string.IsNullOrEmpty(oldNode.FatherId) && oldNode.FatherId != newNode.FatherId)
                {
                    await RemoveParentRelationships(client, oldNode.FatherId, newNode.Id);
                }

                if (!string.IsNullOrEmpty(oldNode.MotherId) && oldNode.MotherId != newNode.MotherId)
                {
                    await RemoveParentRelationships(client, oldNode.MotherId, newNode.Id);
                }

                await UpsertPersonInternal(client, newNode);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RemoveNode(PersonFamilyTreeNode personFamilyTreeNode)
        {
            var client = await Neo4JConnection.GetClient();
            await DeleteNode(client, personFamilyTreeNode);
        }

        internal async Task UpsertPerson(PersonFamilyTreeNode personFamilyTreeNode)
        {
            var client = await Neo4JConnection.GetClient();
            var txClient = (ITransactionalGraphClient)client;
            using var transaction = txClient.BeginTransaction();

            try
            {
                await UpsertPersonInternal(client, personFamilyTreeNode);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static async Task UpsertPersonInternal(
            GraphClient client,
            PersonFamilyTreeNode personFamilyTreeNode)
        {
            await UpsertNode(client, personFamilyTreeNode);
            await EnsureRelationshipsFromProperties(client, personFamilyTreeNode);
        }

        private static async Task UpsertNode(GraphClient client, PersonFamilyTreeNode node)
        {
            var existing = await GetNodeById(client, node.Id);
            if (existing == null)
            {
                await CreateNode(client, node);
                return;
            }

            await client.Cypher
                .Match("(n:Person { Id: $id })")
                .Set("n.Name = $name")
                .Set("n.FatherId = $fatherId")
                .Set("n.MotherId = $motherId")
                .WithParam("id", node.Id)
                .WithParam("name", node.Name)
                .WithParam("fatherId", node.FatherId ?? "")
                .WithParam("motherId", node.MotherId ?? "")
                .ExecuteWithoutResultsAsync();
        }

        private static async Task CreateNode(GraphClient client, PersonFamilyTreeNode node)
        {
            await client.Cypher
                .Create("(n:Person { Id: $id, Name: $name, FatherId: $fatherId, MotherId: $motherId })")
                .WithParam("id", node.Id)
                .WithParam("name", node.Name)
                .WithParam("fatherId", node.FatherId ?? "")
                .WithParam("motherId", node.MotherId ?? "")
                .ExecuteWithoutResultsAsync();
        }

        private static async Task EnsureRelationshipsFromProperties(
            GraphClient client,
            PersonFamilyTreeNode node)
        {
            await CreateParentRelationship(client, node.FatherId, node.Id);
            await CreateChildRelationship(client, node.FatherId, node.Id);
            await CreateParentRelationship(client, node.MotherId, node.Id);
            await CreateChildRelationship(client, node.MotherId, node.Id);

            var children = await client.Cypher
                .Match("(child:Person)")
                .Where("(child.FatherId = $parentId OR child.MotherId = $parentId)")
                .WithParam("parentId", node.Id)
                .Return(child => child.As<PersonFamilyTreeNode>())
                .ResultsAsync;

            foreach (var child in children.Select(NormalizeNode).Where(child => child != null))
            {
                await CreateParentRelationship(client, node.Id, child!.Id);
                await CreateChildRelationship(client, node.Id, child.Id);
            }
        }

        private static async Task CreateParentRelationship(GraphClient client, string? idParent, string idChild)
        {
            if (string.IsNullOrEmpty(idParent))
            {
                return;
            }

            await client.Cypher
                .Match("(a:Person { Id: $idParent }), (b:Person { Id: $idChild })")
                .Merge("(a)-[:PARENT]->(b)")
                .WithParam("idParent", idParent)
                .WithParam("idChild", idChild)
                .ExecuteWithoutResultsAsync();
        }

        private static async Task CreateChildRelationship(GraphClient client, string? idParent, string idChild)
        {
            if (string.IsNullOrEmpty(idParent))
            {
                return;
            }

            await client.Cypher
                .Match("(a:Person { Id: $idParent }), (b:Person { Id: $idChild })")
                .Merge("(b)-[:CHILD]->(a)")
                .WithParam("idParent", idParent)
                .WithParam("idChild", idChild)
                .ExecuteWithoutResultsAsync();
        }

        private static async Task RemoveParentRelationships(GraphClient client, string parentId, string childId)
        {
            await client.Cypher
                .Match("(a:Person { Id: $parentId })-[r:PARENT]->(b:Person { Id: $childId })")
                .Delete("r")
                .WithParam("parentId", parentId)
                .WithParam("childId", childId)
                .ExecuteWithoutResultsAsync();

            await client.Cypher
                .Match("(b:Person { Id: $childId })-[r:CHILD]->(a:Person { Id: $parentId })")
                .Delete("r")
                .WithParam("parentId", parentId)
                .WithParam("childId", childId)
                .ExecuteWithoutResultsAsync();
        }

        private static async Task DeleteNode(GraphClient client, PersonFamilyTreeNode personFamilyTreeNode)
        {
            await client.Cypher
                .Match("(a:Person)")
                .Where("a.Id = $id")
                .WithParam("id", personFamilyTreeNode.Id)
                .DetachDelete("a")
                .ExecuteWithoutResultsAsync();
        }

        private static List<PersonFamilyTreeRelationship> CreateRelationshipsList(
            Dictionary<string, PersonFamilyTreeNode> nodesDictionary,
            List<PersonFamilyTreeNode> nodes)
        {
            var relationships = new List<PersonFamilyTreeRelationship>();
            foreach (var node in nodes)
            {
                if (!string.IsNullOrEmpty(node.MotherId) && nodesDictionary.ContainsKey(node.MotherId))
                {
                    relationships.Add(new PersonFamilyTreeRelationship
                    {
                        ChildId = node.Id,
                        ParentId = node.MotherId
                    });
                }

                if (!string.IsNullOrEmpty(node.FatherId) && nodesDictionary.ContainsKey(node.FatherId))
                {
                    relationships.Add(new PersonFamilyTreeRelationship
                    {
                        ChildId = node.Id,
                        ParentId = node.FatherId
                    });
                }
            }

            return relationships;
        }

        private static async Task<Dictionary<string, PersonFamilyTreeNode>> CollectNodesWithinLevel(
            GraphClient client,
            PersonFamilyTreeNode root,
            uint level)
        {
            var nodes = new Dictionary<string, PersonFamilyTreeNode> { [root.Id] = root };
            if (level == 0)
            {
                return nodes;
            }

            var ancestorQueue = new Queue<(string Id, uint Depth)>();
            EnqueueRelative(ancestorQueue, root.FatherId, 1);
            EnqueueRelative(ancestorQueue, root.MotherId, 1);

            while (ancestorQueue.Count > 0)
            {
                var (relativeId, depth) = ancestorQueue.Dequeue();
                if (depth > level || nodes.ContainsKey(relativeId))
                {
                    continue;
                }

                var relative = await GetNodeById(client, relativeId);
                if (relative == null)
                {
                    continue;
                }

                nodes[relative.Id] = relative;
                await EnsureRelationshipsFromProperties(client, relative);

                if (depth < level)
                {
                    EnqueueRelative(ancestorQueue, relative.FatherId, depth + 1);
                    EnqueueRelative(ancestorQueue, relative.MotherId, depth + 1);
                }
            }

            var descendantQueue = new Queue<(string Id, uint Depth)>();
            descendantQueue.Enqueue((root.Id, 0));
            var visitedDescendants = new HashSet<string> { root.Id };

            while (descendantQueue.Count > 0)
            {
                var (currentId, depth) = descendantQueue.Dequeue();
                if (depth >= level)
                {
                    continue;
                }

                var children = await GetChildrenByParentId(client, currentId);
                foreach (var child in children)
                {
                    if (!visitedDescendants.Add(child.Id))
                    {
                        continue;
                    }

                    nodes[child.Id] = child;
                    await EnsureRelationshipsFromProperties(client, child);

                    var nextDepth = depth + 1;
                    if (nextDepth < level)
                    {
                        descendantQueue.Enqueue((child.Id, nextDepth));
                    }

                    // Include co-parents so child edges render completely at this level.
                    await IncludeCoParent(client, nodes, child.FatherId, currentId);
                    await IncludeCoParent(client, nodes, child.MotherId, currentId);
                }
            }

            return nodes;
        }

        private static async Task IncludeCoParent(
            GraphClient client,
            Dictionary<string, PersonFamilyTreeNode> nodes,
            string? coParentId,
            string knownParentId)
        {
            if (string.IsNullOrEmpty(coParentId) ||
                coParentId == knownParentId ||
                nodes.ContainsKey(coParentId))
            {
                return;
            }

            var coParent = await GetNodeById(client, coParentId);
            if (coParent == null)
            {
                return;
            }

            nodes[coParent.Id] = coParent;
            await EnsureRelationshipsFromProperties(client, coParent);
        }

        private static async Task<List<PersonFamilyTreeNode>> GetChildrenByParentId(
            GraphClient client,
            string parentId)
        {
            var results = await client.Cypher
                .Match("(child:Person)")
                .Where("(child.FatherId = $parentId OR child.MotherId = $parentId)")
                .WithParam("parentId", parentId)
                .Return(child => child.As<PersonFamilyTreeNode>())
                .ResultsAsync;

            return results
                .Select(NormalizeNode)
                .Where(node => node != null)
                .Cast<PersonFamilyTreeNode>()
                .ToList();
        }

        private static async Task<PersonFamilyTreeNode?> GetNodeById(GraphClient client, string personId)
        {
            var results = await client.Cypher
                .Match("(p:Person { Id: $personId })")
                .WithParam("personId", personId)
                .Return(p => p.As<PersonFamilyTreeNode>())
                .ResultsAsync;

            return NormalizeNode(results.FirstOrDefault());
        }

        private static void EnqueueRelative(Queue<(string Id, uint Depth)> queue, string? relativeId, uint depth)
        {
            if (!string.IsNullOrEmpty(relativeId))
            {
                queue.Enqueue((relativeId, depth));
            }
        }

        private static PersonFamilyTreeNode? NormalizeNode(PersonFamilyTreeNode? node)
        {
            if (node == null || string.IsNullOrEmpty(node.Id))
            {
                return null;
            }

            node.FatherId = NullIfEmpty(node.FatherId);
            node.MotherId = NullIfEmpty(node.MotherId);
            return node;
        }

        private static string? NullIfEmpty(string? value) =>
            string.IsNullOrEmpty(value) ? null : value;

        private static PersonFamilyTree EmptyTree() =>
            new()
            {
                Nodes = new List<PersonFamilyTreeNode>(),
                Relationships = new List<PersonFamilyTreeRelationship>()
            };
    }
}
