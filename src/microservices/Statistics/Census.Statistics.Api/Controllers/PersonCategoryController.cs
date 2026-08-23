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
public class PersonCategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public PersonCategoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableRateLimiting(RateLimitingExtensions.AuthenticatedPolicy)]
    [ProducesResponseType(typeof(List<PersonCategoryCounter>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PersonCategoryCounter>>> Get(
        [FromQuery] string? name,
        [FromQuery] string? sex,
        [FromQuery] string? education,
        [FromQuery] string? race)
    {
        var filter = new PersonCategoryFilter
        {
            Name = name ?? string.Empty,
            Sex = sex ?? string.Empty,
            SchoolLevel = education ?? string.Empty,
            Race = race ?? string.Empty
        };
        var result = await _mediator.Send(new PersonCategoryQuery { PersonCategoryFilter = filter });
        return Ok(result);
    }
}
