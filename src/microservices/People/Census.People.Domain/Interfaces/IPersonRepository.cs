using System.Threading.Tasks;
using Census.People.Domain.Entities;

namespace Census.People.Domain.Interfaces
{
    public interface IPersonRepository
    {
        Task<PageResult<Person>> GetPeople(int page, string nameFilter);

        Task<Person> GetPersonById(string id);

        Task Save(Person person, ITransaction? transaction = null);

        Task Update(Person person, ITransaction? transaction = null);

        Task Delete(string id, ITransaction? transaction = null);

        Task<bool> IsAncestorOf(string ancestorId, string descendantId);
    }
}
