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

        /// <summary>
        /// Add official holiday.
        /// </summary>
        /// <param name="command"></param>
        /// <remarks>
        /// If it is holiday with transer day:
        /// ```
        /// POST
        /// {
        ///    "holiday": "2025-01-29",
        ///    "transferDay": "2025-01-29"
        /// }
        ///
        /// If it is holiday without transer day:
        ///
        /// POST
        /// {
        ///    "holiday": "2025-01-29",
        /// }.
        ///
        /// </remarks>
        /// <returns>Calendar entity.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateHoliday([FromBody] AddOfficialHolidayCommand command)
        {
            var result = await mediator.Send(command);

            return Ok(result);
        }
    }
}
