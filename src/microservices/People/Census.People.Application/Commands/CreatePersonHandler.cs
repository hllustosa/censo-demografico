using Census.People.Domain.Entities;
using Census.People.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Census.People.Application.Commands
{
    public class CreatePersonHandler : BasePersonCommandHandler, IRequestHandler<CreatePersonCommand, CreatedPerson>
    {
        IPersonRepository PersonRepository { get; set; }

        public CreatePersonHandler(IPersonRepository personRepository) : base(personRepository)
        {
            PersonRepository = personRepository;
        }

        public async Task<CreatedPerson> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
        {
            Person person = RequestToEntity(request);
            await Validate(person);
            await PersonRepository.Save(person);
            return new CreatedPerson { Id = person.Id };
        }
    }
}
