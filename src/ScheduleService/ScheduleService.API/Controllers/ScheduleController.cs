namespace ScheduleService.API.Controllers
{
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using ScheduleService.Application.UseCases.Commands.Schedule;

    [ApiController]
    [Route("[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IMediator mediator;

        public ScheduleController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// Get user`s month schedule.
        /// </summary>
        /// <returns>Schedule.</returns>
        [HttpGet]
        [Route("schedule/{userId}/{departmentId}/{year:int}/{month:int}")]
        public async Task<IActionResult> GetMonthSchedule(string userId, string departmentId, int year, int month)
        {
            var command = new GetUserMonthScheduleCommand(userId, departmentId, year, month);
            var result = await mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Create or replace work day.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateWorkDay([FromBody] CreateWorkDayManuallyCommand command)
        {
            var result = await mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Delete work day.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteWorkDay([FromBody] DeleteWorkDayCommand command)
        {
            var result = await mediator.Send(command);

            return Ok(result);
        }

        /// <summary>
        /// Generate schedule for department by month.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("generate")]
        public async Task<IActionResult> GenerateMonthSchedule([FromBody] GenerateDepartmentScheduleCommand command)
        {
            await mediator.Send(command);

            return Ok();
        }

        /// <summary>
        /// Delete user`s schedule.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Status.</returns>
        [HttpDelete]
        [Route("deleteMonthSchedule")]
        public async Task<IActionResult> DeleteMonthSchedule([FromBody] DeleteUserMonthScheduleCommand command)
        {
            var result = await mediator.Send(command);

            return Ok(result);
        }
    }
}