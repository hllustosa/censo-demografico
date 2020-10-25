using Census.Statistics.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Census.Statistics.Domain.Interfaces
{
    public interface IPersonPerCityCounterRepository
    {
        Task<List<PersonPerCityCounter>> GetByPersonName(string name);

        Task<PersonPerCityCounter> GetByCity(string city);

        Task<List<string>> GetCities();

        Task Save(ITransaction transaction, PersonPerCityCounter personPerCityCounter);
    }
}
