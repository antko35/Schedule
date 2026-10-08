namespace ScheduleService.API.Controllers
{
    using Application.UseCases.Commands.ScheduleRules;
    using Application.UseCases.Queries.ScheduleRules;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
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
        /// Get all rules for schedule generation.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getAll")]
        public async Task<IActionResult> GetScheduleRules()
        {
            var result = await mediator.Send(new GetAllRulesQuery());

            return Ok(result);
        }

        /// <summary>
        /// Get rules for schedule generation for specific user in department.
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
        /// Create or replace schedule-generation rules for a selected user and month.
        /// </summary>
        /// <param name="userId">The user whose monthly rules will be saved.</param>
        /// <param name="departmentId">The department in which the user works.</param>
        /// <param name="year">The schedule year.</param>
        /// <param name="month">The one-based schedule month.</param>
        /// <param name="command">The complete generation-rule values.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The persisted monthly schedule rules.</returns>
        [HttpPut]
        [Route("rules/{userId}/{departmentId}/{year:int}/{month:int}")]
        public async Task<IActionResult> SaveUserRules(
            string userId,
            string departmentId,
            int year,
            int month,
            [FromBody] SaveGenerationRulesCommand command,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                command with
            {
                UserId = userId,
                DepartmentId = departmentId,
                Year = year,
                Month = month,
            },
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Auto creating userScheduleRules for schedule generation and schedule.
        /// Creating after adding user to department(in user management service).
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("generateRules")]
        public async Task<IActionResult> CreateUserRules([FromBody] CreateScheduleRulesCommand command)
        {
            await mediator.Send(command);

            return Ok();
        }

        /// <summary>
        /// Create rules for schedule generation for specific month.
        /// </summary>
        /// <param name="departmentId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("createRules/{departmentId}/{year:int}/{month:int}")]
        public async Task<IActionResult> CreateRules(string departmentId, int year, int month)
        {
            var command = new CreateRulesForSpecificMonthCommand(departmentId, year, month);

            await mediator.Send(command);

            return Ok();
        }

        /// <summary>
        /// Update schedule-generation rules for a user and month.
        /// Omitted values remain unchanged. Supplying the six periodicity flags as false
        /// selects manual scheduling. At most one periodicity flag can be true.
        /// </summary>
        /// <remarks>
        /// ```
        ///    PATCH {
        ///     "scheduleRulesId": "string",
        ///     "hoursPerMonth": 0,
        ///     "maxHoursPerDay": 0,
        ///     "startWorkDayTime": "string",
        ///     "onlyFirstShift" : false,
        ///     "onlySecondShift" : false,
        ///     "evenDOW": true,
        ///     "unEvenDOW": false,
        ///     "evenDOM": false,
        ///     "unEvenDOM": false
        ///     }.
        ///
        /// evenDOW uses the second shift on Monday, Wednesday, and Friday,
        /// and the first shift on Tuesday and Thursday.
        /// unEvenDOW uses the first shift on Monday, Wednesday, and Friday,
        /// and the second shift on Tuesday and Thursday.
        /// evenDOM and unEvenDOM use the first shift on even and odd month dates respectively.
        /// </remarks>
        /// <param name="command">Updated generation-rule values.</param>
        /// <returns>The persisted schedule-rule entity.</returns>
        [HttpPatch]
        [Route("setRules")]
        public async Task<IActionResult> SetGenerationRules(SetGenerationRulesCommand command)
        {
            var result = await mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Delete all schedules and rules for user.
        /// Calling after deleting user in usen manangement service.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteUserRules(string userId, string departmentId)
        {
            await mediator.Send(new DeleteRulesAndScheduleCommand(userId, departmentId));

            return Ok();
        }
    }
}
