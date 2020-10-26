using Census.FamilyTree.Domain.Entities;
using Census.FamilyTree.Domain.Repository;
using Census.FamilyTree.Infra.Connection;
using Neo4jClient.Transactions;
using System;
using System.Collections.Generic;
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
            var nodesDictionary = await ExecuteQuery(personId, level);
            var nodes = CreateNodesList(nodesDictionary);
            var relationships = CreateRelationshipsList(nodesDictionary, nodes);
            return new PersonFamilyTree() { Nodes = nodes, Relationships = relationships };
        }

        public async Task AddNode(PersonFamilyTreeNode personFamilyTreeNode)
        {
            var client = await Neo4JConnection.GetClient();
            var txClient = (ITransactionalGraphClient)client;
            using (var transaction = txClient.BeginTransaction())
            {
                try
                {
                    await AddNewNode(client, personFamilyTreeNode);
                    await transaction.CommitAsync();
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    throw e;
                }
            }
        }

        public async Task UpdateNode(PersonFamilyTreeNode oldNode, PersonFamilyTreeNode newNode)
        {
            var client = await Neo4JConnection.GetClient();
            var txClient = (ITransactionalGraphClient)client;
            using (var transaction = txClient.BeginTransaction())
            {
                try
                {
                    var children = await GetChildren(oldNode.Id);
                    await DeleteNode(client, oldNode);
                    await AddNewNode(client, newNode);
                    await ReacreateRelationships(newNode, client, children);
                    await transaction.CommitAsync();
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    throw e;
                }
            }
        }

       
        public async Task RemoveNode(PersonFamilyTreeNode personFamilyTreeNode)
        {
            var client = await Neo4JConnection.GetClient();
            var txClient = (ITransactionalGraphClient)client;
            using (var transaction = txClient.BeginTransaction())
            {

                try
                {
                    await DeleteNode(client, personFamilyTreeNode);
                    await transaction.CommitAsync();
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    throw e;
                }
            }
        }

        private static async Task AddNewNode(Neo4jClient.GraphClient client, PersonFamilyTreeNode personFamilyTreeNode)
        {
            var idFather = personFamilyTreeNode.FatherId;
            var idMother = personFamilyTreeNode.MotherId;
            var idChild = personFamilyTreeNode.Id;

            await CreateNode(client, personFamilyTreeNode);
            await CreateParentRelationship(client, idFather, idChild);
            await CreateChildRelationship(client, idFather, idChild);
            await CreateParentRelationship(client, idMother, idChild);
            await CreateChildRelationship(client, idMother, idChild);
        }

        private async static Task CreateNode(Neo4jClient.GraphClient client, PersonFamilyTreeNode personFamilyTreeNode)
        {
            await client.Cypher.Create("(n:Person { Id: $id, Name: $name, FatherId: $fatherId, MotherId: $motherId })")
                    .WithParam("id", personFamilyTreeNode.Id)
                    .WithParam("name", personFamilyTreeNode.Name)
                    .WithParam("fatherId", personFamilyTreeNode.FatherId)
                    .WithParam("motherId", personFamilyTreeNode.MotherId)
                    .ExecuteWithoutResultsAsync();
        }


        private async static Task CreateParentRelationship(Neo4jClient.GraphClient client, string idParent, string idChild)
        {
            if (String.IsNullOrEmpty(idParent))
                return;

            await client.Cypher.Match("(a:Person),(b:Person)")
                    .Where("a.Id = $idParent AND b.Id = $idChild")
                    .WithParam("idParent", idParent)
                    .WithParam("idChild", idChild)
                    .Create("(a)-[r:PARENT]->(b)")
                    .ExecuteWithoutResultsAsync();
        }

        private async static Task CreateChildRelationship(Neo4jClient.GraphClient client, string idParent, string idChild)
        {
            if (String.IsNullOrEmpty(idParent))
                return;

            await client.Cypher.Match("(a:Person),(b:Person)")
                    .Where("a.Id = $idParent AND b.Id = $idChild")
                    .WithParam("idParent", idParent)
                    .WithParam("idChild", idChild)
                    .Create("(b)-[r:CHILD]->(a)")
                    .ExecuteWithoutResultsAsync();
        }


        private async static Task DeleteNode(Neo4jClient.GraphClient client, PersonFamilyTreeNode personFamilyTreeNode)
        {
            await client.Cypher.Match("(a:Person)")
                    .Where("a.Id = $id")
                    .WithParam("id", personFamilyTreeNode.Id)
                    .DetachDelete("a")
                    .ExecuteWithoutResultsAsync();
        }

        private static List<PersonFamilyTreeRelationship> CreateRelationshipsList(Dictionary<string, PersonFamilyTreeNode> nodesDictionary, List<PersonFamilyTreeNode> nodes)
        {
            var relationships = new List<PersonFamilyTreeRelationship>();
            foreach (var node in nodes)
            {
                if (node.MotherId != null && nodesDictionary.ContainsKey(node.MotherId))
                {
                    relationships.Add(new PersonFamilyTreeRelationship()
                    {
                        ChildId = node.Id,
                        ParentId = node.MotherId
                    });
                }

                if (node.FatherId != null && nodesDictionary.ContainsKey(node.FatherId))
                {
                    relationships.Add(new PersonFamilyTreeRelationship()
                    {
                        ChildId = node.Id,
                        ParentId = node.FatherId
                    });
                }
            }

            return relationships;
        }

        private static List<PersonFamilyTreeNode> CreateNodesList(Dictionary<string, PersonFamilyTreeNode> nodesDictionary)
        {
            var nodes = new List<PersonFamilyTreeNode>();
            foreach (var key in nodesDictionary.Keys)
            {
                nodes.Add(nodesDictionary[key]);
            }

            return nodes;
        }

        private async Task<List<string>> GetChildren(string personId)
        {
            var client = await Neo4JConnection.GetClient();

            var results = await client.Cypher.Match("(p:Person { Id: $personId } )-[:PARENT]->(c:Person)") 
                            .WithParam("personId", personId)
                            .Return((p, c) => new
                            {
                                child = c.As<PersonFamilyTreeNode>(),
                            }).ResultsAsync;

            var children = new List<string>();
            foreach (var record in results)
            {
                children.Add(record.child.Id);
            }

            return children;
        }

        private async Task<Dictionary<string, PersonFamilyTreeNode>> ExecuteQuery(string personId, uint level)
        {
            var client = await Neo4JConnection.GetClient();

            //[ALERTA] Parâmetros não funcionando no limitador de saltos. Ex: código com parâmetro [*1..$level]
            //causa exceção. Alternativamente, estou formatando a string diretamente com o valor de level
            //Como a variável level é uint, não é possível ocorrer uma injeção de comando através dela.
            var results = await client.Cypher.Match("(p:Person { Id: $personId } )-[*1.."+ level + "]->(c:Person)")
                            //PersonId que é inseguro consegue ser configurado via parâmetro 
                            //sem causar exceção
                            //.WithParam("level", level.ToString())
                            .WithParam("personId", personId)
                            .Return((p, c) => new
                            {
                                p1 = p.As<PersonFamilyTreeNode>(),
                                p2 = c.As<PersonFamilyTreeNode>(),
                            }).ResultsAsync;

            var nodesDictionary = new Dictionary<string, PersonFamilyTreeNode>();
            foreach (var record in results)
            {
                nodesDictionary[record.p1.Id] = record.p1;
                nodesDictionary[record.p2.Id] = record.p2;
            }

            return nodesDictionary;
        }

        private static async Task ReacreateRelationships(PersonFamilyTreeNode newNode, Neo4jClient.GraphClient client, List<string> children)
        {
            foreach (var child in children)
            {
                await CreateParentRelationship(client, newNode.Id, child);
                await CreateChildRelationship(client, newNode.Id, child);
            }
        }
    }
}
