using MediatR;
using System.Collections.Generic;

namespace Census.Statistics.Application.Queries
{
    public class CitiesQuery : IRequest<List<string>>
    {

    }
}
