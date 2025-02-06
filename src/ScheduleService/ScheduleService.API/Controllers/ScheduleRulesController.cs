using MediatR;
using Microsoft.AspNetCore.Mvc;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.Application.UseCases.Queries.ScheduleRules;

namespace ScheduleService.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScheduleRulesController : ControllerBase
    {
        private readonly IMediator mediator;

        public ScheduleRulesController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// Get ruler for schedule generation.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="departmentId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("rules/{userId}/{departmentId}/{year:int}/{month:int}")]
        public async Task<IActionResult> GetUserRules(string userId, string departmentId, int year, int month)
        {
            var result = await mediator.Send(new GetUserRulesQuery(userId, departmentId, month, year));

            return Ok(result);
        }

        /// <summary>
        /// Auto creating userScheduleRules for schedule generation.
        /// Creating after adding user to department(in user management service).
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateUserRules([FromBody] CreateScheduleRulesCommand command)
        {
            await mediator.Send(command);

            return Ok();
        }

        /// <summary>
        /// Set default rules for schedule rules.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ClearRules()
        {
            return Ok();
        }
    }
}
