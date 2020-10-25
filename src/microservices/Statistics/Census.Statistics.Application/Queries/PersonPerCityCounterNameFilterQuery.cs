using Census.Statistics.Domain.Entities;
using MediatR;
using System.Collections.Generic;

namespace Census.Statistics.Application.Queries
{
    public class PersonPerCityCounterNameFilterQuery : IRequest<List<PersonPerCityCounter>>
    {
        public string NameFilter { get; set; }
    }
}
