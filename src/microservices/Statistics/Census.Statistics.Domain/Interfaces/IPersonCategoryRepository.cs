using System.Collections.Generic;
using System.Threading.Tasks;
using Census.Statistics.Domain.Entities;

namespace Census.Statistics.Domain.Interfaces
{
    public interface IPersonCategoryRepository
    {
        Task<List<PersonCategoryCounter>> GetPersonCategoryCounters(PersonCategoryFilter personCategoryFilter);

        Task Save(ITransaction transaction, PersonCategoryCounter personCategoryCounter);
    }
}
