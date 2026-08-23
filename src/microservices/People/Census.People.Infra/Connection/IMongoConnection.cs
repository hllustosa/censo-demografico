using Census.People.Domain.Entities;
using MongoDB.Driver;

namespace Census.People.Infra.Connection
{
    public interface IMongoConnection
    {
        MongoClient GetClient();

        IMongoDatabase GetDatabase();

        IMongoCollection<Person> GetPeopleCollection();
    }
}
