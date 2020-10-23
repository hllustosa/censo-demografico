using Census.People.Domain.Entities;
using MongoDB.Driver;


namespace Census.People.Infra.Connection
{
    public interface IMongoConnection
    {
        IMongoCollection<Person> GetPeopleCollection();
    }
}
