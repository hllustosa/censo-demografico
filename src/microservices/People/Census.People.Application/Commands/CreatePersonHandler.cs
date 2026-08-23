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
        private readonly ITransactionManager _transactionManager;

        public CreatePersonHandler(
            IPersonRepository personRepository,
            IIntegrationEventPublisher eventPublisher,
            ITransactionManager transactionManager) : base(personRepository)
        {
            _personRepository = personRepository;
            _eventPublisher = eventPublisher;
            _transactionManager = transactionManager;
        }

        public async Task<CreatedPerson> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
        {
            Person person = RequestToEntity(request);
            await Validate(person);

            var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
            try
            {
                await _personRepository.Save(person, transaction);
                await _eventPublisher.PublishAsync(CreateEvent(person), transaction, cancellationToken);
                await _transactionManager.CommitAsync(transaction, cancellationToken);
            }
            catch
            {
                await _transactionManager.RollbackAsync(transaction, cancellationToken);
                throw;
            }

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
