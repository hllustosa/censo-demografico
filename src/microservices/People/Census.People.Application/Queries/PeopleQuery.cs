using Census.People.Domain.Entities;
using MediatR;

namespace Census.People.Application.Queries
{
    public class PeopleQuery : IRequest<PageResult<Person>>
    {
        public string NameFilter { get; set; }

        public int Page { get; set; }
    }
}
