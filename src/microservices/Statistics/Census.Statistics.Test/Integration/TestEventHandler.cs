using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Census.Shared.Bus.Event;
using Census.Statistics.Application;
using Census.Statistics.Application.Events;
using Census.Statistics.Domain.Entities;
using Census.Statistics.Domain.Interfaces;
using Census.Statistics.Infra.Connection;
using Census.Statistics.Infra.Repository;
using Census.Statistics.Infra.Service;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Census.Statistics.Test.Integration
{
    [Collection("StatisticsIntegration")]
    public class TestEventHandler
    {
        private readonly IPersonCategoryRepository _personCategoryRepository;
        private readonly IPersonPerCityCounterRepository _personPerCityCounterRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly Mock<INotificationSender> _notificationSender = new();

        public TestEventHandler(MongoFixture mongoFixture)
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = mongoFixture.ConnectionString
                })
                .Build();

            var connection = new MongoConnection(config);
            var guidGenerator = new GuidGenerator();
            _personCategoryRepository = new PersonCategoryRepository(connection, guidGenerator);
            _personPerCityCounterRepository = new PersonPerCityCounterRepository(connection, guidGenerator);
            _transactionManager = new MongoTransactionManager(connection);
        }

        [Fact]
        public async Task TestPersonCreateEventHandler()
        {
            var handler = new PersonCreatedEventHandler(
                _personCategoryRepository,
                _personPerCityCounterRepository,
                _transactionManager,
                _notificationSender.Object);

            var @event = new PersonCreatedEvent { Person = CreatePerson1() };

            var result = await GetCategory(@event.Person);
            var expected = result[0];
            expected.Count++;

            await handler.Handle(@event);

            result = await GetCategory(@event.Person);
            Assert.Equal(expected.Count, result[0].Count);
        }

        [Fact]
        public async Task TestPersonUpdatedEventHandler()
        {
            var handler = new PersonUpdatedEventHandler(
                _personCategoryRepository,
                _personPerCityCounterRepository,
                _transactionManager);

            var @event = new PersonUpdatedEvent
            {
                OldPersonData = CreatePerson1(),
                NewPersonData = CreatePerson2()
            };

            var resultOld = await GetCategory(@event.OldPersonData);
            var resultNew = await GetCategory(@event.NewPersonData);

            var expectedOld = resultOld[0];
            var expectedNew = resultNew[0];

            expectedOld.Count = Math.Max(expectedOld.Count - 1, 0);
            expectedNew.Count++;

            await handler.Handle(@event);

            resultOld = await GetCategory(@event.OldPersonData);
            resultNew = await GetCategory(@event.NewPersonData);

            Assert.Equal(expectedOld.Count, resultOld[0].Count);
            Assert.Equal(expectedNew.Count, resultNew[0].Count);
        }

        [Fact]
        public async Task TestPersonDeletedEventHandler()
        {
            var handler = new PersonDeletedEventHandler(
                _personCategoryRepository,
                _personPerCityCounterRepository,
                _transactionManager);

            var @event = new PersonDeletedEvent { Person = CreatePerson2() };

            var result = await GetCategory(@event.Person);
            var expected = result[0];
            expected.Count = Math.Max(expected.Count - 1, 0);

            await handler.Handle(@event);

            result = await GetCategory(@event.Person);
            Assert.Equal(expected.Count, result[0].Count);
        }

        private static PersonDTO CreatePerson1() =>
            new()
            {
                Name = "João",
                Sex = "M",
                Education = "Ensino Médio",
                Race = "Pardo(a)",
                Address = new AddressDTO { City = "City" }
            };

        private static PersonDTO CreatePerson2() =>
            new()
            {
                Name = "João",
                Sex = "M",
                Education = "Ensino Fundamental",
                Race = "Branco(a)",
                Address = new AddressDTO { City = "City" }
            };

        private Task<List<PersonCategoryCounter>> GetCategory(PersonDTO person) =>
            _personCategoryRepository.GetPersonCategoryCounters(new PersonCategoryFilter
            {
                Sex = person.Sex,
                SchoolLevel = person.Education,
                Race = person.Race
            });
    }
}
