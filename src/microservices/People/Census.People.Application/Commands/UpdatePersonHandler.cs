using Census.People.Domain.Entities;
using Census.People.Domain.Interfaces;
using FluentValidation.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Census.People.Application.Commands
{
    public class UpdatePersonHandler : BasePersonCommandHandler, IRequestHandler<UpdatePersonCommand>
    {
        IPersonRepository PersonRepository { get; set; }

        public UpdatePersonHandler(IPersonRepository personRepository) : base(personRepository)
        {
            PersonRepository = personRepository;
        }

        public async Task<Unit> Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
        {
            Person person = RequestToEntity(request);
            await CheckIfExists(person.Id, "Id");
            await Validate(person);
            await PersonRepository.Update(person);
            return Unit.Value;
        }
    }
}
