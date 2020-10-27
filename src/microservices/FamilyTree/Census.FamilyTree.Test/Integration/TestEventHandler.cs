using Census.FamilyTree.Application.Events;
using Census.FamilyTree.Domain.Entities;
using Census.FamilyTree.Domain.Repository;
using Census.FamilyTree.Infra.Connection;
using Census.FamilyTree.Infra.Repository;
using Census.Shared.Bus.Event;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Census.FamilyTree.Test.Integration
{
    public class TestEventHandler
    {
        INeo4jConnection Connection { get; set; }

        IPersonFamilyTreeRepository PersonFamilyTreeRepository { get; set; }

        public TestEventHandler()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            Connection = new Neo4jConnection(config);
            PersonFamilyTreeRepository = new PersonFamilyTreeRepository(Connection);
        }

        [Fact]
        public async Task TestPersonCreateEventHandler()
        {
            //Arrange
            await SetupData();
            var handler = new PersonCreatedEventHandler(PersonFamilyTreeRepository);
            var @event = new PersonCreatedEvent()
            {
                Person = CreatePerson1()
            };

            //Act
            await handler.Handle(@event);

            //Assert
            var result = await PersonFamilyTreeRepository.GetFamilyTree("6", 1);
            Assert.NotEmpty(result.Nodes.Where(item => item.Id == "3"));
            Assert.NotEmpty(result.Nodes.Where(item => item.Id == "4"));
            Assert.NotEmpty(result.Nodes.Where(item => item.Id == "6"));
        }

        [Fact]
        public async Task TestPersonUpdateEventHandler()
        {
            //Arrange
            await SetupData();
            var handler = new PersonUpdatedEventHandler(PersonFamilyTreeRepository);
            var @event = new PersonUpdatedEvent()
            {
                OldPersonData = CreatePerson2(),
                NewPersonData = CreatePerson3(),
            };

            //Act
            await handler.Handle(@event);

            //Assert
            var result = await PersonFamilyTreeRepository.GetFamilyTree("3", 2);
            Assert.NotEmpty(result.Nodes.Where(item => item.Id == "5"));
            Assert.Empty(result.Nodes.Where(item => item.Id == "1"));
            Assert.Empty(result.Nodes.Where(item => item.Id == "2"));
        }


        [Fact]
        public async Task TestPersonDeleteEventHandler()
        {
            //Arrange
            await SetupData();
            var handler = new PersonDeletedEventHandler(PersonFamilyTreeRepository);
            var @event = new PersonDeletedEvent()
            {
                Person = CreatePerson2(),
            };

            //Act
            await handler.Handle(@event);

            //Assert
            var result = await PersonFamilyTreeRepository.GetFamilyTree("5", 2);
            Assert.Empty(result.Nodes.Where(item => item.Id == "3"));

            result = await PersonFamilyTreeRepository.GetFamilyTree("1", 2);
            Assert.Empty(result.Nodes.Where(item => item.Id == "3"));

            result = await PersonFamilyTreeRepository.GetFamilyTree("2", 2);
            Assert.Empty(result.Nodes.Where(item => item.Id == "3"));

        }

        private async Task SetupData()
        {
            var client = await Connection.GetClient();
            using (var transaction = client.BeginTransaction())
            {
                await client.Cypher.Match("(n:Person)").DetachDelete("n").ExecuteWithoutResultsAsync();
                await transaction.CommitAsync();
            }
                
            await PersonFamilyTreeRepository.AddNode(new PersonFamilyTreeNode()
            {
                Id = "1",
                Name = "Cacionilha"
            });

            await PersonFamilyTreeRepository.AddNode(new PersonFamilyTreeNode()
            {
                Id = "2",
                Name = "Murilo"
            });

            await PersonFamilyTreeRepository.AddNode(new PersonFamilyTreeNode()
            {
                Id = "3",
                Name = "Vera",
                MotherId = "1",
                FatherId = "2",
            });

            await PersonFamilyTreeRepository.AddNode(new PersonFamilyTreeNode()
            {
                Id = "4",
                Name = "Hermano"
            });

            await PersonFamilyTreeRepository.AddNode(new PersonFamilyTreeNode()
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
