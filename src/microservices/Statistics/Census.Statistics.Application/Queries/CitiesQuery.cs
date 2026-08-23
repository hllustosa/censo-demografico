using System.Collections.Generic;
using MediatR;

namespace Census.Statistics.Application.Queries
{
    public class CitiesQuery : IRequest<List<string>>
    {

    }
}
