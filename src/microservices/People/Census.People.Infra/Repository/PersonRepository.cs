using Census.People.Domain.Entities;
using Census.People.Domain.Interfaces;
using Census.People.Infra.Connection;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Census.People.Infra.Repository
{
    public class PersonRepository : IPersonRepository
    {
        readonly int PAGE_SIZE = 10;

        IMongoConnection MongoConnection { get; set; }

        IGuidGenerator GuidGenerator { get; set; }

        public PersonRepository(IMongoConnection mongoConnection, IGuidGenerator guidGenerator)
        {
            MongoConnection = mongoConnection;
            GuidGenerator = guidGenerator;
        }

        public async Task<PageResult<Person>> GetPeople(int page, string nameFilter)
        {
            IEnumerable<Person> people;
            var collection = MongoConnection.GetPeopleCollection();

            if (HasDefinedNameFilter(nameFilter))
            {
                people = await GetPeopleFilteredByName(nameFilter, collection);
            }
            else
            {
                people = await GetAllPeople(collection);
            }

            return CreatePagedResult(page, people);
        }


        public async Task<Person> GetPersonById(string id)
        {
            var collection = MongoConnection.GetPeopleCollection();
            var result = await collection.FindAsync(item => item.Id == id);
            return result.FirstOrDefault();
        }

        public async Task Save(Person person)
        {
            person.Id = CreateId();
            var collection = MongoConnection.GetPeopleCollection();
            await collection.InsertOneAsync(person);
        }

        public async Task Update(Person person)
        {
            var collection = MongoConnection.GetPeopleCollection();
            await collection.ReplaceOneAsync(item => item.Id == person.Id, person);
        }

        public async Task Delete(string id)
        {
            var collection = MongoConnection.GetPeopleCollection();
            await collection.DeleteOneAsync(filter => filter.Id == id);
        }

        private PageResult<Person> CreatePagedResult(int page, IEnumerable<Person> people)
        {
            var list = people.ToList();

            return new PageResult<Person>()
            {
                Items = GetPageItems(page, list),
                TotalItems = list.Count()
            };
        }

        private List<Person> GetPageItems(int page, IEnumerable<Person> people)
        {
            return people.Skip(PAGE_SIZE * (page - 1)).Take(PAGE_SIZE).ToList();
        }

        private static async Task<IEnumerable<Person>> GetAllPeople(IMongoCollection<Person> collection)
        {
            var result = await collection.FindAsync(new BsonDocument());
            return result.ToEnumerable();
        }

        private static async Task<IEnumerable<Person>> GetPeopleFilteredByName(string nameFilter, 
            IMongoCollection<Person> collection)
        {
            var filter = BuildFilter(nameFilter);
            var result = await collection.FindAsync(filter);
            return result.ToEnumerable();
        }

        private static FilterDefinition<Person> BuildFilter(string search)
        {
            var builder = Builders<Person>.Filter;
            return builder.Regex("name", "^" + search + ".*");
        }

        private string CreateId()
        {
            return GuidGenerator.GenerateGuid();
        }

        private static bool HasDefinedNameFilter(string nameFilter)
        {
            return !String.IsNullOrEmpty(nameFilter);
        }

    }
}
