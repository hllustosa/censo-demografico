using Census.People.Domain.Interfaces;
using Census.People.Infra.Connection;
using MongoDB.Driver;

namespace Census.People.Infra.Repository
{
    public class MongoSession : ITransaction
    {
        public required IClientSessionHandle Session { get; init; }
    }

    public class MongoTransactionManager : ITransactionManager
    {
        private readonly IMongoConnection _mongoConnection;

        public MongoTransactionManager(IMongoConnection mongoConnection)
        {
            _mongoConnection = mongoConnection;
        }

        public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var session = await _mongoConnection.GetClient().StartSessionAsync(cancellationToken: cancellationToken);
            session.StartTransaction();
            return new MongoSession { Session = session };
        }

        public async Task CommitAsync(ITransaction transaction, CancellationToken cancellationToken = default)
        {
            var session = ((MongoSession)transaction).Session;
            if (session.IsInTransaction)
            {
                await session.CommitTransactionAsync(cancellationToken);
            }

            session.Dispose();
        }

        public async Task RollbackAsync(ITransaction transaction, CancellationToken cancellationToken = default)
        {
            var session = ((MongoSession)transaction).Session;
            try
            {
                if (session.IsInTransaction)
                {
                    await session.AbortTransactionAsync(cancellationToken);
                }
            }
            finally
            {
                session.Dispose();
            }
        }
    }
}
