using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Census.FamilyTree.Application.Events;
using Census.FamilyTree.Domain.Entities;
using Census.FamilyTree.Domain.Repository;
using Census.FamilyTree.Infra.Connection;
using Census.FamilyTree.Infra.Repository;
using Census.Shared.Bus.Event;
using Microsoft.Extensions.Configuration;
using Neo4jClient.Transactions;
using Xunit;

namespace Census.FamilyTree.Test.Integration
{
    [Collection("Neo4jIntegration")]
    public class TestEventHandler
    {
        private readonly INeo4jConnection _connection;
        private readonly IPersonFamilyTreeRepository _personFamilyTreeRepository;

        public TestEventHandler(Neo4jFixture fixture)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddInMemoryCollection(fixture.ConfigurationOverrides())
                .Build();
            _connection = new Neo4jConnection(config);
            _personFamilyTreeRepository = new PersonFamilyTreeRepository(_connection);
        }

        [Fact]
        public async Task TestPersonCreateEventHandler()
        {
            await SetupData();
            var handler = new PersonCreatedEventHandler(_personFamilyTreeRepository);
            var @event = new PersonCreatedEvent()
            {
                Person = CreatePerson1()
            };

            await handler.Handle(@event);

            var result = await _personFamilyTreeRepository.GetFamilyTree("6", 1);
            Assert.NotEmpty(result.Nodes.Where(item => item.Id == "3"));
            Assert.NotEmpty(result.Nodes.Where(item => item.Id == "4"));
            Assert.NotEmpty(result.Nodes.Where(item => item.Id == "6"));
        }

        [Fact]
        public async Task TestPersonUpdateEventHandler()
        {
            await SetupData();
            var handler = new PersonUpdatedEventHandler(_personFamilyTreeRepository);
            var @event = new PersonUpdatedEvent()
            {
                OldPersonData = CreatePerson2(),
                NewPersonData = CreatePerson3(),
            };

            await handler.Handle(@event);

            var result = await _personFamilyTreeRepository.GetFamilyTree("3", 2);
            Assert.NotEmpty(result.Nodes.Where(item => item.Id == "5"));
            Assert.DoesNotContain(result.Nodes, item => item.Id == "1");
            Assert.DoesNotContain(result.Nodes, item => item.Id == "2");
        }

        [Fact]
        public async Task TestPersonDeleteEventHandler()
        {
            await SetupData();
            var handler = new PersonDeletedEventHandler(_personFamilyTreeRepository);
            var @event = new PersonDeletedEvent()
            {
                Person = CreatePerson2(),
            };

            await handler.Handle(@event);

            var result = await _personFamilyTreeRepository.GetFamilyTree("5", 2);
            Assert.DoesNotContain(result.Nodes, item => item.Id == "3");

            result = await _personFamilyTreeRepository.GetFamilyTree("1", 2);
            Assert.DoesNotContain(result.Nodes, item => item.Id == "3");

            result = await _personFamilyTreeRepository.GetFamilyTree("2", 2);
            Assert.DoesNotContain(result.Nodes, item => item.Id == "3");
        }

        private async Task SetupData()
        {
            var client = await _connection.GetClient();
            var txClient = (ITransactionalGraphClient)client;
            using (var transaction = txClient.BeginTransaction())
            {
                await client.Cypher.Match("(n:Person)").DetachDelete("n").ExecuteWithoutResultsAsync();
                await transaction.CommitAsync();
            }

            await _personFamilyTreeRepository.AddNode(new PersonFamilyTreeNode()
            {
                Id = "1",
                Name = "Cacionilha"
            });

            await _personFamilyTreeRepository.AddNode(new PersonFamilyTreeNode()
            {
                Id = "2",
                Name = "Murilo"
            });

            await _personFamilyTreeRepository.AddNode(new PersonFamilyTreeNode()
            {
                Id = "3",
                Name = "Vera",
                MotherId = "1",
                FatherId = "2",
            });

            await _personFamilyTreeRepository.AddNode(new PersonFamilyTreeNode()
            {
                Id = "4",
                Name = "Hermano"
            });

            await _personFamilyTreeRepository.AddNode(new PersonFamilyTreeNode()
            {
                Id = "5",
                Name = "Lourenço",
                MotherId = "3",
                FatherId = "4"
            });
        }

        private static PersonDTO CreatePerson1()
        {
            return new PersonDTO()
            {
                Id = "6",
                Name = "Juliana",
                Sex = "F",
                Education = "Ensino Superior",
                Race = "Branco(a)",
                Address = new AddressDTO()
                {
                    City = "City",
                },
                MotherId = "3",
                FatherId = "4"
            };
        }

        private static PersonDTO CreatePerson2()
        {
            return new PersonDTO()
            {
                Id = "3",
                Name = "Vera",
                Sex = "M",
                Education = "Ensino Superior",
                Race = "Branco(a)",
                Address = new AddressDTO()
                {
                    City = "City",
                },
                MotherId = "1",
                FatherId = "2"
            };
        }

        private static PersonDTO CreatePerson3()
        {
            return new PersonDTO()
            {
                Id = "3",
                Name = "Vera",
                Sex = "M",
                Education = "Ensino Superior",
                Race = "Branco(a)",
                Address = new AddressDTO()
                {
                    City = "City",
                },
                MotherId = "",
                FatherId = ""
            };
        }
    }
}
