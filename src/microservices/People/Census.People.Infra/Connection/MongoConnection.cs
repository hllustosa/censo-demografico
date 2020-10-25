using Census.People.Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Census.People.Infra.Connection
{
    public class MongoConnection : IMongoConnection
    {
        readonly string MONGO_DATABASE = "peopledb";

        readonly string MONGO_COLLECTION = "people";

        MongoClient MongoClient { get; set; }

        public MongoConnection(IConfiguration configuration)
        {
            MongoClient = new MongoClient(configuration.GetConnectionString("DefaultConnection"));
        }

        public IMongoCollection<Person> GetPeopleCollection()
        {
            var database = MongoClient.GetDatabase(MONGO_DATABASE);
            return database.GetCollection<Person>(MONGO_COLLECTION);
        }
    }
}
