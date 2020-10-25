using Census.Statistics.Application.Queries;
using Census.Statistics.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Census.Statistics.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonCategoryController : Controller
    {
        readonly IMediator Mediator;

        public PersonCategoryController(IMediator mediator)
        {
            Mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<PersonCategoryCounter>>> Get([FromQuery] string name, 
            [FromQuery] string sex,
            [FromQuery] string education,
            [FromQuery] string race)
        {
            var filter = new PersonCategoryFilter() { Name = name, Sex = sex, SchoolLevel= education, Race = race};
            var result = await Mediator.Send(new PersonCategoryQuery() { PersonCategoryFilter = filter });
            return Ok(result);
        }
    }
}