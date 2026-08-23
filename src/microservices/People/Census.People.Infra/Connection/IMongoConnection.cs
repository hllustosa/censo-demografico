using Census.People.Domain.Entities;
using MongoDB.Driver;

namespace Census.People.Infra.Connection
{
    public interface IMongoConnection
    {
        IMongoDatabase GetDatabase();

        IMongoCollection<Person> GetPeopleCollection();
    }
}
