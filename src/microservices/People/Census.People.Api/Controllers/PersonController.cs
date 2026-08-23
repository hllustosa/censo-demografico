using Asp.Versioning;
using Census.People.Application.Commands;
using Census.People.Application.Queries;
using Census.People.Domain.Entities;
using Census.Shared.Auth;
using Census.Shared.Web;
using Census.Shared.Web.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Census.People.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class PersonController : ControllerBase
{
    private readonly IMediator _mediator;

    public PersonController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = CensusPolicies.CanReadPeople)]
    [EnableRateLimiting(RateLimitingExtensions.AuthenticatedPolicy)]
    [ProducesResponseType(typeof(PageResult<Person>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PageResult<Person>>> Get([FromQuery] int page, [FromQuery] string? name)
    {
        var result = await _mediator.Send(new PeopleQuery { Page = page, NameFilter = name ?? string.Empty });
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = CensusPolicies.CanReadPeople)]
    [EnableRateLimiting(RateLimitingExtensions.AuthenticatedPolicy)]
    [ProducesResponseType(typeof(Person), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Person>> Get(string id)
    {
        var result = await _mediator.Send(new PersonByIdQuery { Id = id });
        if (result is null)
        {
            throw new NotFoundException("Pessoa não encontrada.");
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = CensusPolicies.CanManagePeople)]
    [EnableRateLimiting(RateLimitingExtensions.AuthenticatedPolicy)]
    [ProducesResponseType(typeof(CreatedPerson), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreatedPerson>> Post([FromBody] CreatePersonCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(Get), new { id = result.Id, version = "1.0" }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = CensusPolicies.CanManagePeople)]
    [EnableRateLimiting(RateLimitingExtensions.AuthenticatedPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(string id, [FromBody] UpdatePersonCommand command)
    {
        command.Id = id;
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = CensusPolicies.CanManagePeople)]
    [EnableRateLimiting(RateLimitingExtensions.AuthenticatedPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        await _mediator.Send(new DeletePersonCommand { Id = id });
        return NoContent();
    }
}
