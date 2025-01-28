using MediatR;
using Microsoft.AspNetCore.Mvc;
using ScheduleService.Application.UseCases.Commands.Calendar;

namespace ScheduleService.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalendarController : ControllerBase
    {
        private readonly IMediator mediator;

        public CalendarController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateHoliday([FromBody] AddOfficialHolidayCommand command)
        {
            await mediator.Send(command);

            return Ok();
        }
    }
}
