using Census.People.Domain.Entities;
using Census.People.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Census.People.Application.Queries
{
    public class PeopleQueryHandler : IRequestHandler<PeopleQuery, PageResult<Person>>
    {
        IPersonRepository PersonRepository { get; set; }

        public PeopleQueryHandler(IPersonRepository personRepository)
        {
            PersonRepository = personRepository;
        }

        public async Task<PageResult<Person>> Handle(PeopleQuery request, CancellationToken cancellationToken)
        {
            return await PersonRepository.GetPeople(request.Page, request.NameFilter);
        }
    }
}
