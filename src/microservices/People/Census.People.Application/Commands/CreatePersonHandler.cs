using Census.People.Domain.Entities;
using Census.People.Domain.Interfaces;
using Census.Shared.Bus.Event;
using Census.Shared.Bus.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Census.People.Application.Commands
{
    public class CreatePersonHandler : BasePersonCommandHandler, IRequestHandler<CreatePersonCommand, CreatedPerson>
    {
        IPersonRepository PersonRepository { get; set; }

        IEventBus EventBus { get; set; }

        public CreatePersonHandler(IPersonRepository personRepository, IEventBus eventBus) : base(personRepository)
        {
            PersonRepository = personRepository;
            EventBus = eventBus;
        }

        public async Task<CreatedPerson> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
        {
            Person person = RequestToEntity(request);
            await Validate(person);
            await PersonRepository.Save(person);
            EventBus.Publish(CreateEvent(person));
            return new CreatedPerson { Id = person.Id };
        }

        private PersonCreatedEvent CreateEvent(Person person)
        {
            return new PersonCreatedEvent()
            {
                Person = ToDTO(person)
            };
        }
    }
}
