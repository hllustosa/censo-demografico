using System.Threading;
using System.Threading.Tasks;
using Census.People.Domain.Entities;
using Census.People.Domain.Interfaces;
using MediatR;

namespace Census.People.Application.Queries
{
    public class PersonByIdQueryHandler : IRequestHandler<PersonByIdQuery, Person>
    {
        IPersonRepository PersonRepository { get; set; }

        public PersonByIdQueryHandler(IPersonRepository personRepository)
        {
            PersonRepository = personRepository;
        }

        public async Task<Person> Handle(PersonByIdQuery request, CancellationToken cancellationToken)
        {
            return await PersonRepository.GetPersonById(request.Id);
        }
    }
}
