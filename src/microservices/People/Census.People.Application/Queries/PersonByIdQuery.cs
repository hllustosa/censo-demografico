using Census.People.Domain.Entities;
using MediatR;

namespace Census.People.Application.Queries
{
    public class PersonByIdQuery : IRequest<Person>
    {
        public string Id { get; set; }
    }
}
