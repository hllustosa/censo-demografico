using Microsoft.AspNetCore.Mvc;
using MediatR;
using Census.FamilyTree.Application.Queries;
using Census.FamilyTree.Domain.Entities;
using System.Threading.Tasks;

namespace Census.FamilyTree.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FamilyTreeController : ControllerBase
    {
        IMediator Mediator { get; set; }

        public FamilyTreeController(IMediator mediator)
        {
            Mediator = mediator;
        }

        [HttpGet("id")]
        public async Task<ActionResult<PersonFamilyTree>> GetAsync(string id, [FromQuery]uint level)
        {
            var result = await Mediator.Send(new FamilyTreeQuery() { PersonId = id, Level = level  });
            return Ok(result);
        }
    }
}
