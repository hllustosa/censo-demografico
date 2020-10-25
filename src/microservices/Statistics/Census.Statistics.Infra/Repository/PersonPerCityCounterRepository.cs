using Census.Statistics.Domain.Entities;
using Census.Statistics.Domain.Interfaces;
using Census.Statistics.Infra.Connection;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Census.Statistics.Infra.Repository
{
    public class PersonPerCityCounterRepository : IPersonPerCityCounterRepository
    {
        IMongoConnection MongoConnection { get; set; }

        IGuidGenerator GuidGenerator { get; set; }

        public PersonPerCityCounterRepository(IMongoConnection mongoConnection, IGuidGenerator guidGenerator)
        {
            MongoConnection = mongoConnection;
            GuidGenerator = guidGenerator;
        }

        public async Task<PersonPerCityCounter> GetByCity(string city)
        {
            var collection = MongoConnection.GetPersonPerCityCounterCollection();
            var filter = Builders<PersonPerCityCounter>.Filter.Eq(x => x.City, city);
            var result = await collection.FindAsync(filter);
            return result.ToList().DefaultIfEmpty(CreateDefaultForCity(city)).First();
        }

        public async Task<List<PersonPerCityCounter>> GetByPersonName(string name)
        {
            var collection = MongoConnection.GetPersonPerCityCounterCollection();
            var filter = Builders<PersonPerCityCounter>.Filter.Where(x => x.PersonNameCounters.ContainsKey(name));
            var result = await collection.FindAsync(filter);
            return result.ToList();
        }

        public async Task<List<string>> GetCities()
        {
            var collection = MongoConnection.GetPersonPerCityCounterCollection();
            var filter = Builders<PersonPerCityCounter>.Filter.Empty;
            var result = await collection.FindAsync(filter);
            var cities = result.ToEnumerable().Select( x => x.City ).ToList();
            return cities;
        }

        public async Task Save(ITransaction transaction, PersonPerCityCounter personPerCityCounter)
        {
            var session = ((MongoSession)transaction).Session;
            var collection = MongoConnection.GetPersonPerCityCounterCollection();
            var filter = Builders<PersonPerCityCounter>.Filter.Eq(x => x.City, personPerCityCounter.City);

            var currentDocument = await GetByCity(personPerCityCounter.City);
            personPerCityCounter.Id = currentDocument?.Id ?? GuidGenerator.GenerateGuid();

            await collection.ReplaceOneAsync(
                filter: filter,
                options: new ReplaceOptions { IsUpsert = true },
                replacement: personPerCityCounter,
                session: session);
        }

        private PersonPerCityCounter CreateDefaultForCity(string city)
        {
            return new PersonPerCityCounter()
            { 
                City = city,
                Count = 0,
            };
        }
    }
}
