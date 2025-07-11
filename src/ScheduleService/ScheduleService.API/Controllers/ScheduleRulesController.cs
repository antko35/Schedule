﻿using MediatR;
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
        /// Get rules for schedule generation.
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
        /// Set hours per day and max working hours per day.
        /// Set when user work`s first shift.
        /// Ex: If user works first shift on even days of week set: EvenDOW: true.
        /// If user works first shift on even days of month set: EvenDOM: true.
        /// If user work`s only first shift set "onlyFirstShift : true".
        /// If user work`s only second shift set "onlySecondShift : true".
        /// </summary>
        /// <remarks>
        /// ```
        ///    PATCH {
        ///     "scheduleRulesId": "string",
        ///     "hoursPerMonth": 0,
        ///     "maxHoursPerDay": 0,
        ///     "startWorkDayTime": "string",
        ///     "onlyFirstShift" : true,
        ///     "onlySecondShift" : true,
        ///     "evenDOW": true,
        ///     "unEvenDOW": true,
        ///     "evenDOM": true,
        ///     "unEvenDOM": true
        ///     }.
        /// </remarks>
        /// <returns></returns>
        [HttpPatch]
        [Route("setRules")]
        public async Task<IActionResult> SetGenerationRules(SetGenerationRulesCommand command)
        {
            await mediator.Send(command);

            return Ok();
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
