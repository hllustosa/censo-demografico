using Asp.Versioning;
using Census.Shared.Auth;
using Census.Shared.Web;
using Census.Statistics.Application.Queries;
using Census.Statistics.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Census.Statistics.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = CensusPolicies.CanViewDashboard)]
[Produces("application/json")]
public class PerCityCategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public PerCityCategoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("cities/{city}/counter")]
    [EnableRateLimiting(RateLimitingExtensions.AuthenticatedPolicy)]
    [ProducesResponseType(typeof(PersonPerCityCounter), StatusCodes.Status200OK)]
    public async Task<ActionResult<PersonPerCityCounter>> GetPerCityCategory(string city)
    {
        var result = await _mediator.Send(new PersonPerCityCounterCityFilterQuery { CityNameFilter = city });
        return Ok(result);
    }

    [HttpGet("counters/{name}")]
    [EnableRateLimiting(RateLimitingExtensions.AuthenticatedPolicy)]
    [ProducesResponseType(typeof(List<PersonPerCityCounter>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PersonPerCityCounter>>> GetPerCityCategoryWithNameFilter(string name)
    {
        var result = await _mediator.Send(new PersonPerCityCounterNameFilterQuery { NameFilter = name });
        return Ok(result);
    }

    [HttpGet("cities")]
    [EnableRateLimiting(RateLimitingExtensions.AuthenticatedPolicy)]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<string>>> Get()
    {
        var result = await _mediator.Send(new CitiesQuery());
        return Ok(result);
    }
}
