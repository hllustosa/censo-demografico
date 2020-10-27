using Census.Statistics.Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Census.Statistics.Infra.Connection
{
    public class MongoConnection : IMongoConnection
    {
        readonly string MONGO_DATABASE = "statsdb";

        readonly string MONGO_CATEGORY_COLLECTION = "categories";

        readonly string MONGO_CITY_CATEGORY_COLLECTION = "citycategories";

        MongoClient MongoClient { get; set; }

        public MongoConnection(IConfiguration configuration)
        {
            var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
            ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);
            MongoClient = new MongoClient(configuration.GetConnectionString("DefaultConnection"));
        }

        public MongoClient GetClient()
        {
            return MongoClient;
        }

        public IMongoCollection<PersonCategoryCounter> GetPersonCategoriesCollection()
        {
            var database = MongoClient.GetDatabase(MONGO_DATABASE);
            return database.GetCollection<PersonCategoryCounter>(MONGO_CATEGORY_COLLECTION);
        }

        public IMongoCollection<PersonPerCityCounter> GetPersonPerCityCounterCollection()
        {
            var database = MongoClient.GetDatabase(MONGO_DATABASE);
            return database.GetCollection<PersonPerCityCounter>(MONGO_CITY_CATEGORY_COLLECTION);
        }
    }
}
