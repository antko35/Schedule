namespace ScheduleService.API.Controllers
{
    using Application.UseCases.Commands.Calendar;
    using Application.UseCases.Queries.Calendar;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
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
        /// Gets all holidays with their transfer dates.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>The complete holiday calendar ordered by holiday date.</returns>
        [HttpGet("holidays")]
        public async Task<IActionResult> GetHolidays(CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new GetHolidaysQuery(), cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Get holidays by year.
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("holidays/{year:int}")]
        public async Task<IActionResult> GetYearHolidays(int year)
        {
            var responce = await mediator.Send(new GetYearHolidaysQuery(year));

            return Ok(responce);
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
            var responce = await mediator.Send(new GetMonthHolidaysQuery(year, month));

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
            var responce = await mediator.Send(new GetMonthTransferDaysQuery(year, month));

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

        /// <summary>
        /// Delete holiday.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteHoliday([FromBody] DeleteHolidayCommand command)
        {
            await mediator.Send(command);

            return Ok();
        }

        /// <summary>
        /// Deletes one or more holidays from the shared calendar.
        /// </summary>
        /// <param name="command">The holiday dates to delete.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <returns>An empty successful response when all selected holidays are deleted.</returns>
        [HttpDelete("batch")]
        public async Task<IActionResult> DeleteHolidays(
            [FromBody] DeleteHolidaysCommand command,
            CancellationToken cancellationToken)
        {
            await mediator.Send(command, cancellationToken);

            return NoContent();
        }
    }
}
