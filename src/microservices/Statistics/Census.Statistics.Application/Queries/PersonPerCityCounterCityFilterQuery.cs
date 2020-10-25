using Census.Statistics.Domain.Entities;
using MediatR;
using System.Collections.Generic;

namespace Census.Statistics.Application.Queries
{
    public class PersonPerCityCounterCityFilterQuery : IRequest<PersonPerCityCounter>
    {
        public string CityNameFilter { get; set; }
    }
}
