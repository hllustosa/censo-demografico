using Census.People.Domain.Values;
using MediatR;

namespace Census.People.Application.Commands
{
    public class CreatePersonCommand : BasePersonCommand, IRequest<CreatedPerson> { }

    public class CreatedPerson
    {
        public string Id { get; set; }
    }
}
