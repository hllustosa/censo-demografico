using Census.People.Application.Services;
using Census.People.Domain.Entities;
using Census.People.Domain.Interfaces;
using Census.Shared.Bus.Event;
using MediatR;

namespace Census.People.Application.Commands
{
    public class DeletePersonHandler : BasePersonCommandHandler, IRequestHandler<DeletePersonCommand>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public DeletePersonHandler(IPersonRepository personRepository, IIntegrationEventPublisher eventPublisher) : base(personRepository)
        {
            _personRepository = personRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task Handle(DeletePersonCommand request, CancellationToken cancellationToken)
        {
            await CheckIfExists(request.Id, "Id");
            var person = await _personRepository.GetPersonById(request.Id);
            await _personRepository.Delete(request.Id);
            await _eventPublisher.PublishAsync(CreateEvent(person), cancellationToken);
        }

        private PersonDeletedEvent CreateEvent(Person person)
        {
            return new PersonDeletedEvent()
            {
                Person = ToDTO(person)
            };
        }
    }
}
