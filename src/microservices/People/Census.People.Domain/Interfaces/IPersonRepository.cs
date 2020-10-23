using Census.People.Domain.Entities;
using System.Threading.Tasks;

namespace Census.People.Domain.Interfaces
{
    public interface IPersonRepository
    {
        Task<PageResult<Person>> GetPeople(int page, string nameFilter);

        Task<Person> GetPersonById(string id);

        Task Save(Person person);

        Task Update(Person person);

        Task Delete(string id);
    }
}
