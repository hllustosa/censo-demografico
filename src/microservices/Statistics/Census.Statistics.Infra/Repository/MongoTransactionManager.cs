using Census.Statistics.Domain.Interfaces;
using Census.Statistics.Infra.Connection;
using MongoDB.Driver;
using System;

namespace Census.Statistics.Infra.Repository
{
    public class MongoTransactionManager : ITransactionManager
    {
        IMongoConnection MongoConnection { get; set; }

        public MongoTransactionManager(IMongoConnection mongoConnection)
        {
            MongoConnection = mongoConnection;
        }

        public ITransaction BeginTransaction()
        {
            var client = MongoConnection.GetClient();
            var session = client.StartSession();
            try
            {
                session.StartTransaction();
                return new MongoSession() { Session = session };
            }
            catch(NotSupportedException)
            {
                return new MongoSession() { Session = session };
            }
        }

        public void CommitTransaction(ITransaction transaction)
        {
            var session = ((MongoSession)transaction).Session;
            if(session.IsInTransaction)
            {
                session.CommitTransaction();
            }
        }

        public void RollBackTransaction(ITransaction transaction)
        {
            var session = ((MongoSession)transaction).Session;
            if (session.IsInTransaction)
            {
                session.AbortTransaction();
            }
        }
    }
}
