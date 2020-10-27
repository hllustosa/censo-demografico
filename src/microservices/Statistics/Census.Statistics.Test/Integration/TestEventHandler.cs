using Census.People.Infra.Repository;
using Census.Shared.Bus.Event;
using Census.Statistics.Application.Events;
using Census.Statistics.Domain.Entities;
using Census.Statistics.Domain.Interfaces;
using Census.Statistics.Infra.Connection;
using Census.Statistics.Infra.Repository;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Census.Statistics.Test.Integration
{
    public class TestEventHandler
    {   
        IMongoConnection Connection { get; set; }

        IPersonCategoryRepository PersonCategoryRepository { get; set; }

        IPersonPerCityCounterRepository PersonPerCityCounterRepository { get; set; }

        IGuidGenerator GuidGenerator { get; set; }

        ITransactionManager TransactionManager { get; set; }

        public TestEventHandler()
        {
            Connection = new MongoConnection("mongodb://mongo:27017");
            GuidGenerator = new GuidGenerator();
            PersonCategoryRepository = new PersonCategoryRepository(Connection, GuidGenerator);
            PersonPerCityCounterRepository = new PersonPerCityCounterRepository(Connection, GuidGenerator);
            TransactionManager = new MongoTransactionManager(Connection);
        }

        [Fact]
        public async Task TestPersonCreateEventHandler()
        {
            //Arrange
            var handler = new PersonCreatedEventHandler(PersonCategoryRepository,
                PersonPerCityCounterRepository, TransactionManager);

            var @event = new PersonCreatedEvent()
            {
                Person = CreatePerson1()
            };

            //Act
            var result = await GetCategory(@event.Person);
            var expected = result[0];
            expected.Count++;

            await handler.Handle(@event);

            result = await GetCategory(@event.Person);
            var obtained = result[0];

            //Assert
            Assert.Equal(expected.Count, obtained.Count);
        }


        [Fact]
        public async Task TestPersonUpdatedEventHandler()
        {
            //Arrange
            var handler = new PersonUpdatedEventHandler(PersonCategoryRepository,
                PersonPerCityCounterRepository, TransactionManager);

            var @event = new PersonUpdatedEvent()
            {
                OldPersonData = CreatePerson1(),
                NewPersonData = CreatePerson2()
            };

            //Act
            var resultOld = await GetCategory(@event.OldPersonData);
            var resultNew = await GetCategory(@event.NewPersonData);

            var expectedOld = resultOld[0];
            var expectedNew = resultNew[0];

            expectedOld.Count--;
            expectedNew.Count++;

            await handler.Handle(@event);

            resultOld = await GetCategory(@event.OldPersonData);
            resultNew = await GetCategory(@event.NewPersonData);

            var obtainedOld = resultOld[0];
            var obtainedNew = resultNew[0];

            //Assert
            Assert.Equal(expectedOld.Count, obtainedOld.Count);
            Assert.Equal(expectedNew.Count, obtainedNew.Count);
        }

        [Fact]
        public async Task TestPersonDeletedEventHandler()
        {
            //Arrange
            var handler = new PersonDeletedEventHandler(PersonCategoryRepository,
                PersonPerCityCounterRepository, TransactionManager);

            var @event = new PersonDeletedEvent()
            {
                Person = CreatePerson2(),
            };

            //Act
            var result = await GetCategory(@event.Person);
            var expected = result[0];
            expected.Count = Math.Max(expected.Count - 1, 0);

            await handler.Handle(@event);

            result = await GetCategory(@event.Person);
            var obtained = result[0];

            //Assert
            Assert.Equal(expected.Count, obtained.Count);

        }

        private static PersonDTO CreatePerson1()
        {
            return new PersonDTO()
            {
                Name = "João",
                Sex = "M",
                Education = "Ensino Médio",
                Race = "Pardo(a)",
                Address = new AddressDTO()
                {
                    City = "City",
                }

            };
        }

        private static PersonDTO CreatePerson2()
        {
            return new PersonDTO()
            {
                Name = "João",
                Sex = "M",
                Education = "Ensino Fundamental",
                Race = "Branco(a)",
                Address = new AddressDTO()
                {
                    City = "City",
                }

            };
        }

        private async Task<System.Collections.Generic.List<PersonCategoryCounter>> GetCategory(PersonDTO person)
        {
            var result = await PersonCategoryRepository.GetPersonCategoryCounters(new PersonCategoryFilter()
            {
                Sex = person.Sex,
                SchoolLevel = person.Education,
                Race = person.Race
            });
            return result;
        }
    }
}
