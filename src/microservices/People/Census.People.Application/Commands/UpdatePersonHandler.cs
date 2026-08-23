using Census.People.Application.Services;
using Census.People.Domain.Entities;
using Census.People.Domain.Interfaces;
using Census.Shared.Bus.Event;
using MediatR;

namespace Census.People.Application.Commands
{
    public class UpdatePersonHandler : BasePersonCommandHandler, IRequestHandler<UpdatePersonCommand>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public UpdatePersonHandler(IPersonRepository personRepository, IIntegrationEventPublisher eventPublisher) : base(personRepository)
        {
            _personRepository = personRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
        {
            Person person = RequestToEntity(request);
            await CheckIfExists(person.Id, "Id");
            await Validate(person);

            var oldPerson = await _personRepository.GetPersonById(person.Id);
            await _personRepository.Update(person);
            await _eventPublisher.PublishAsync(CreateEvent(oldPerson, person), cancellationToken);
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
