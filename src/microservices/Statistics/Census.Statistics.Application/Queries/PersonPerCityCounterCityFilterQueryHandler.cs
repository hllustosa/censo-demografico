using Census.Statistics.Domain.Entities;
using Census.Statistics.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Census.Statistics.Application.Queries
{
    public class PersonPerCityCounterCityFilterQueryHandler : IRequestHandler<PersonPerCityCounterCityFilterQuery, PersonPerCityCounter>
    {
        IPersonPerCityCounterRepository PersonPerCityCounterRepository { get; set; }

        public PersonPerCityCounterCityFilterQueryHandler(IPersonPerCityCounterRepository personPerCityCounterRepository)
        {
            PersonPerCityCounterRepository = personPerCityCounterRepository;
        }

        public async Task<PersonPerCityCounter> Handle(PersonPerCityCounterCityFilterQuery request, CancellationToken cancellationToken)
        {
            return await PersonPerCityCounterRepository.GetByCity(request.CityNameFilter);
        }
    }
}
