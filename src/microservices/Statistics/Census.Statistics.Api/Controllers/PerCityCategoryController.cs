using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Census.Statistics.Application.Queries;
using Census.Statistics.Domain.Entities;

namespace Census.Statistics.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PerCityCategoryController : Controller
    {
        readonly IMediator Mediator;

        public PerCityCategoryController(IMediator mediator)
        {
            Mediator = mediator;
        }

        [HttpGet("cities/{city}/counter")]
        public async Task<ActionResult<PersonPerCityCounter>> GetPerCityCategory(string city)
        {
            var result = await Mediator.Send(new PersonPerCityCounterCityFilterQuery() { CityNameFilter = city });
            return Ok(result);
        }

        [HttpGet("counters/{name}")]
        public async Task<ActionResult<List<PersonPerCityCounter>>> GetPerCityCategoryWithNameFilter(string name)
        {
            var result = await Mediator.Send(new PersonPerCityCounterNameFilterQuery() { NameFilter = name});
            return Ok(result);
        }

        [HttpGet("cities")]
        public async Task<ActionResult<List<string>>> Get()
        {
            var result = await Mediator.Send(new CitiesQuery());
            return Ok(result);
        }
    }
}