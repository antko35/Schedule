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
        /// Get holidays with transfer days for month.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("holidays/{year:int}/{month:int}")]
        public async Task<IActionResult> GetMonthHolidays(int year, int month)
        {
            var responce = await mediator.Send(new GetMonthHolidaysCommand(year, month));

            return Ok(responce);
        }

        /// <summary>
        /// Get transfer days for month.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("transferDays/{year:int}/{month:int}")]
        public async Task<IActionResult> GetYearHolidays(int year, int month)
        {
            var responce = await mediator.Send(new GetMonthTransferDaysCommand(year, month));

            return Ok(responce);
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
