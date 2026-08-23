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
        private readonly ITransactionManager _transactionManager;

        public UpdatePersonHandler(
            IPersonRepository personRepository,
            IIntegrationEventPublisher eventPublisher,
            ITransactionManager transactionManager) : base(personRepository)
        {
            _personRepository = personRepository;
            _eventPublisher = eventPublisher;
            _transactionManager = transactionManager;
        }

        public async Task Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
        {
            Person person = RequestToEntity(request);
            await CheckIfExists(person.Id, "Id");
            await Validate(person);

            var oldPerson = await _personRepository.GetPersonById(person.Id);

            var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
            try
            {
                await _personRepository.Update(person, transaction);
                await _eventPublisher.PublishAsync(CreateEvent(oldPerson, person), transaction, cancellationToken);
                await _transactionManager.CommitAsync(transaction, cancellationToken);
            }
            catch
            {
                await _transactionManager.RollbackAsync(transaction, cancellationToken);
                throw;
            }
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
