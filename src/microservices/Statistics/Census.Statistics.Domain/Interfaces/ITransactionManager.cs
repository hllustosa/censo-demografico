using System;
using System.Collections.Generic;
using System.Text;

namespace Census.Statistics.Domain.Interfaces
{
    public interface ITransaction {}

    public interface ITransactionManager
    {
        ITransaction BeginTransaction();

        void CommitTransaction(ITransaction transaction);

        void RollBackTransaction(ITransaction transaction);
    }
}
