using Census.Statistics.Domain.Interfaces;
using MongoDB.Driver;

namespace Census.Statistics.Infra.Repository
{
    public class MongoSession : ITransaction
    {
        public IClientSessionHandle Session { get; set; } 
    }
}
