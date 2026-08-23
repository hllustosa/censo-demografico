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
        private readonly ITransactionManager _transactionManager;

        public DeletePersonHandler(
            IPersonRepository personRepository,
            IIntegrationEventPublisher eventPublisher,
            ITransactionManager transactionManager) : base(personRepository)
        {
            _personRepository = personRepository;
            _eventPublisher = eventPublisher;
            _transactionManager = transactionManager;
        }

        public async Task Handle(DeletePersonCommand request, CancellationToken cancellationToken)
        {
            await CheckIfExists(request.Id, "Id");
            var person = await _personRepository.GetPersonById(request.Id);

            var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
            try
            {
                await _personRepository.Delete(request.Id, transaction);
                await _eventPublisher.PublishAsync(CreateEvent(person), transaction, cancellationToken);
                await _transactionManager.CommitAsync(transaction, cancellationToken);
            }
            catch
            {
                await _transactionManager.RollbackAsync(transaction, cancellationToken);
                throw;
            }
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
