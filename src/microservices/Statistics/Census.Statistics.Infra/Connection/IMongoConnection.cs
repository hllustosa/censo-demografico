using Census.Statistics.Domain.Entities;
using MongoDB.Driver;

namespace Census.Statistics.Infra.Connection
{
    public interface IMongoConnection
    {
        MongoClient GetClient();

        IMongoDatabase GetDatabase();

        IMongoCollection<PersonCategoryCounter> GetPersonCategoriesCollection();

        IMongoCollection<PersonPerCityCounter> GetPersonPerCityCounterCollection();
    }
}
