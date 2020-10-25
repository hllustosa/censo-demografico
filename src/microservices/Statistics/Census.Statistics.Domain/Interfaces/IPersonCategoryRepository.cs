using Census.Statistics.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Census.Statistics.Domain.Interfaces
{
    public interface IPersonCategoryRepository
    {
        Task<List<PersonCategoryCounter>> GetPersonCategoryCounters(PersonCategoryFilter personCategoryFilter);

        Task Save(ITransaction transaction, PersonCategoryCounter personCategoryCounter);
    }
}
