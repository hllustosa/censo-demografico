using MediatR;

namespace Census.People.Application.Commands
{
    public class DeletePersonCommand : IRequest
    {
        public string Id { get; set; }
    }
}
