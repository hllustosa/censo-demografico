using Asp.Versioning;
using Census.FamilyTree.Application.Queries;
using Census.FamilyTree.Domain.Entities;
using Census.Shared.Auth;
using Census.Shared.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Census.FamilyTree.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = CensusPolicies.CanViewFamilyTree)]
[Produces("application/json")]
public class FamilyTreeController : ControllerBase
{
    private readonly IMediator _mediator;

    public FamilyTreeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    [EnableRateLimiting(RateLimitingExtensions.AuthenticatedPolicy)]
    [ProducesResponseType(typeof(PersonFamilyTree), StatusCodes.Status200OK)]
    public async Task<ActionResult<PersonFamilyTree>> GetAsync(string id, [FromQuery] uint level)
    {
        var result = await _mediator.Send(new FamilyTreeQuery { PersonId = id, Level = level });
        return Ok(result);
    }
}
