using MediatR;
using Microsoft.AspNetCore.Mvc;
using ScheduleService.Application.UseCases.Commands.Schedule;

namespace ScheduleService.API.Controllers
{
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
        public async Task CreateWorkDay([FromBody] CreateWorkDayManuallyCommand command)
        {
            await mediator.Send(command);
        }

        /// <summary>
        /// Delete work day.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task DeleteWorkDay([FromBody] DeleteWorkDayCommand command)
        {
            await mediator.Send(command);
        }

        [HttpPost]
        [Route("departmentId")]
        public async Task GenerateMonthSchedule([FromBody] GenerateDepartmentScheduleCommand command)
        {
            await mediator.Send(command);
        }
    }
}