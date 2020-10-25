using Census.Statistics.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Census.Statistics.Application.Queries
{
    public class CitiesQueryHandler : IRequestHandler<CitiesQuery, List<string>>
    {
        IPersonPerCityCounterRepository PersonPerCityCounterRepository { get; set; }

        public CitiesQueryHandler(IPersonPerCityCounterRepository personPerCityCounterRepository)
        {
            PersonPerCityCounterRepository = personPerCityCounterRepository;
        }

        public async Task<List<string>> Handle(CitiesQuery request, CancellationToken cancellationToken)
        {
            return await PersonPerCityCounterRepository.GetCities();
        }
    }
}
