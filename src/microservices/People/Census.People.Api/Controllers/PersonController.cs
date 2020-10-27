using Census.People.Application.Commands;
using Census.People.Application.Queries;
using Census.People.Domain.Entities;
using Census.Shared.Bus.Event;
using Census.Shared.Bus.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Census.People.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        readonly IMediator Mediator;

        public PersonController(IMediator mediator)
        {
            Mediator = mediator;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<PageResult<Person>>> Get([FromQuery] int page, [FromQuery] string name)
        {
            var result = await Mediator.Send(new PeopleQuery() { Page = page, NameFilter = name});
            return Ok(result);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Person>> Get(string id)
        {
            var result = await Mediator.Send(new PersonByIdQuery() { Id = id });
            return Ok(result);
        }

        // POST api/values
        [HttpPost]
        public async Task<CreatedPerson> Post([FromBody] CreatePersonCommand command)
        {
            var result = await Mediator.Send(command);
            return result;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] UpdatePersonCommand command)
        {
            command.Id = id;
            await Mediator.Send(command);
            return Ok();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await Mediator.Send(new DeletePersonCommand() { Id = id});
            return NoContent();
        }
    }
}
