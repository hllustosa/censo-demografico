using Census.People.Domain.Entities;
using Census.People.Domain.Interfaces;
using Census.Shared.Bus.Event;
using Census.Shared.Bus.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Census.People.Application.Commands
{
    public class DeletePersonHandler : BasePersonCommandHandler, IRequestHandler<DeletePersonCommand>
    {
        IPersonRepository PersonRepository { get; set; }

        IEventBus EventBus { get; set; }

        public DeletePersonHandler(IPersonRepository personRepository, IEventBus eventBus) : base(personRepository)
        {
            PersonRepository = personRepository;
            EventBus = eventBus;
        }

        public async Task<Unit> Handle(DeletePersonCommand request, CancellationToken cancellationToken)
        {
            await CheckIfExists(request.Id, "Id");
            var person = await PersonRepository.GetPersonById(request.Id);
            await PersonRepository.Delete(request.Id);
            EventBus.Publish(CreateEvent(person));
            return Unit.Value;
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
