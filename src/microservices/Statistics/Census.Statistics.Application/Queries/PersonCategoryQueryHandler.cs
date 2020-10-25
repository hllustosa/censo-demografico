using Census.Statistics.Domain.Entities;
using Census.Statistics.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Census.Statistics.Application.Queries
{
    public class PersonCategoryQueryHandler : IRequestHandler<PersonCategoryQuery, List<PersonCategoryCounter>>
    {
        IPersonCategoryRepository PersonCategoryRepository { get; set; }

        public PersonCategoryQueryHandler(IPersonCategoryRepository personCategoryRepository)
        {
            PersonCategoryRepository = personCategoryRepository;
        }

        public async Task<List<PersonCategoryCounter>> Handle(PersonCategoryQuery request, CancellationToken cancellationToken)
        {
            return await PersonCategoryRepository.GetPersonCategoryCounters(request.PersonCategoryFilter);
        }
    }
}
