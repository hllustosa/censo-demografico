using System.Collections.Generic;
using Census.Statistics.Domain.Entities;
using MediatR;

namespace Census.Statistics.Application.Queries
{
    public class PersonPerCityCounterCityFilterQuery : IRequest<PersonPerCityCounter>
    {
        public string CityNameFilter { get; set; }
    }
}
