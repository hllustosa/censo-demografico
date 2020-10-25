using Census.Statistics.Domain.Entities;
using Census.Statistics.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Census.Statistics.Application.Queries
{
    public class PersonPerCityCounterNameFilterQueryHandler : IRequestHandler<PersonPerCityCounterNameFilterQuery, List<PersonPerCityCounter>>
    {
        IPersonPerCityCounterRepository PersonPerCityCounterRepository { get; set; }

        public PersonPerCityCounterNameFilterQueryHandler(IPersonPerCityCounterRepository personPerCityCounterRepository)
        {
            PersonPerCityCounterRepository = personPerCityCounterRepository;
        }

        public async Task<List<PersonPerCityCounter>> Handle(PersonPerCityCounterNameFilterQuery request, CancellationToken cancellationToken)
        {
            return await PersonPerCityCounterRepository.GetByPersonName(request.NameFilter);
        }
    }
}
