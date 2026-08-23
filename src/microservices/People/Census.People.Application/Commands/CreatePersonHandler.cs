using Census.People.Application.Services;
using Census.People.Domain.Entities;
using Census.People.Domain.Interfaces;
using Census.Shared.Bus.Event;
using MediatR;

namespace Census.People.Application.Commands
{
    public class CreatePersonHandler : BasePersonCommandHandler, IRequestHandler<CreatePersonCommand, CreatedPerson>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public CreatePersonHandler(IPersonRepository personRepository, IIntegrationEventPublisher eventPublisher) : base(personRepository)
        {
            _personRepository = personRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<CreatedPerson> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
        {
            Person person = RequestToEntity(request);
            await Validate(person);
            await _personRepository.Save(person);
            await _eventPublisher.PublishAsync(CreateEvent(person), cancellationToken);
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
