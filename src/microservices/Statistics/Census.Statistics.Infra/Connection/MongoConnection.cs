using Census.Statistics.Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Census.Statistics.Infra.Connection
{
    public class MongoConnection : IMongoConnection
    {
        private const string MongoDatabase = "statsdb";
        private const string MongoCategoryCollection = "categories";
        private const string MongoCityCategoryCollection = "citycategories";

        private readonly MongoClient _mongoClient;

        public MongoConnection(IConfiguration configuration)
        {
            var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
            ConventionRegistry.Register("IgnoreExtraElements", conventionPack, _ => true);
            _mongoClient = new MongoClient(configuration.GetConnectionString("DefaultConnection"));
        }

        public MongoClient GetClient() => _mongoClient;

        public IMongoDatabase GetDatabase() => _mongoClient.GetDatabase(MongoDatabase);

        public IMongoCollection<PersonCategoryCounter> GetPersonCategoriesCollection()
        {
            return GetDatabase().GetCollection<PersonCategoryCounter>(MongoCategoryCollection);
        }

        public IMongoCollection<PersonPerCityCounter> GetPersonPerCityCounterCollection()
        {
            return GetDatabase().GetCollection<PersonPerCityCounter>(MongoCityCategoryCollection);
        }
    }
}
