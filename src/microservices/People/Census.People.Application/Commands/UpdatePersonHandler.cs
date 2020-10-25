using Census.People.Domain.Entities;
using Census.People.Domain.Interfaces;
using Census.Shared.Bus.Event;
using Census.Shared.Bus.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Census.People.Application.Commands
{
    public class UpdatePersonHandler : BasePersonCommandHandler, IRequestHandler<UpdatePersonCommand>
    {
        IPersonRepository PersonRepository { get; set; }

        IEventBus EventBus { get; set; }

        public UpdatePersonHandler(IPersonRepository personRepository, IEventBus eventBus) : base(personRepository)
        {
            PersonRepository = personRepository;
            EventBus = eventBus;
        }

        public async Task<Unit> Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
        {
            Person person = RequestToEntity(request);
            await CheckIfExists(person.Id, "Id");
            await Validate(person);
            
            var oldPerson = await PersonRepository.GetPersonById(person.Id);
            await PersonRepository.Update(person);
            EventBus.Publish(CreateEvent(oldPerson, person));
            return Unit.Value;
        }

        private PersonUpdatedEvent CreateEvent(Person oldPerson, Person newPerson)
        {
            return new PersonUpdatedEvent()
            {
                OldPersonData = ToDTO(oldPerson),
                NewPersonData = ToDTO(newPerson)
            };
        }
    }
}
