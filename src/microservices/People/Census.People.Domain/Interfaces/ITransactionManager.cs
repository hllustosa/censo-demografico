namespace Census.People.Domain.Interfaces
{
    public interface ITransaction
    {
    }

    public interface ITransactionManager
    {
        Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task CommitAsync(ITransaction transaction, CancellationToken cancellationToken = default);

        Task RollbackAsync(ITransaction transaction, CancellationToken cancellationToken = default);
    }
}
