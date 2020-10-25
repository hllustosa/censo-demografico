using Census.Statistics.Domain.Entities;
using MediatR;
using System.Collections.Generic;

namespace Census.Statistics.Application.Queries
{
    public class PersonCategoryQuery : IRequest<List<PersonCategoryCounter>>
    {
        public PersonCategoryFilter PersonCategoryFilter { get; set; }
    }
}
