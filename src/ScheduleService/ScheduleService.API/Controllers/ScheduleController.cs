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

        [HttpPost]
        [Route("departmentId")]
        public async Task<IActionResult> GenerateMonthSchedule([FromBody] GenerateDepartmentScheduleCommand command)
        {
            await mediator.Send(command);

            return Ok();
        }
    }
}