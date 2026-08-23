using System.Collections.Generic;
using Census.Statistics.Domain.Entities;
using MediatR;

namespace Census.Statistics.Application.Queries
{
    public class PersonCategoryQuery : IRequest<List<PersonCategoryCounter>>
    {
        public PersonCategoryFilter PersonCategoryFilter { get; set; }
    }
}
