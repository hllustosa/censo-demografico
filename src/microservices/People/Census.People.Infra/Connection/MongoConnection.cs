using Census.People.Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Census.People.Infra.Connection
{
    public class MongoConnection : IMongoConnection
    {
        private const string MongoDatabase = "peopledb";
        private const string MongoCollection = "people";

        private readonly MongoClient _mongoClient;

        public MongoConnection(IConfiguration configuration)
        {
            _mongoClient = new MongoClient(configuration.GetConnectionString("DefaultConnection"));
        }

        public IMongoDatabase GetDatabase() => _mongoClient.GetDatabase(MongoDatabase);

        public IMongoCollection<Person> GetPeopleCollection()
        {
            return GetDatabase().GetCollection<Person>(MongoCollection);
        }
    }
}
