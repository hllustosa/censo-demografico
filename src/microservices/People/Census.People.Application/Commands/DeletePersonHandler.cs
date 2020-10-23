using Census.People.Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Census.People.Application.Commands
{
    public class DeletePersonHandler : IRequestHandler<DeletePersonCommand>
    { 
        IPersonRepository PersonRepository { get; set; }

        public DeletePersonHandler(IPersonRepository personRepository)
        {
            PersonRepository = personRepository;
        }

        public async Task<Unit> Handle(DeletePersonCommand request, CancellationToken cancellationToken)
        {
            await CheckIfExists(request.Id, "Id");
            await PersonRepository.Delete(request.Id);
            return Unit.Value;
        }

        private async Task CheckIfExists(string id, string field)
        {
            var person = await PersonRepository.GetPersonById(id);
            if (person == null) throw new ValidationException(
                new List<ValidationFailure>() { new ValidationFailure(field, "Valor Inválido") });
        }
    }
}
