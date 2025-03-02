namespace ScheduleService.Application.UseCases.QueryHandlers.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Application.UseCases.Commands.Schedule;
    using ScheduleService.Application.UseCases.Queries.Calendar;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class GetMonthHolidaysQueryHandler : IRequestHandler<GetMonthHolidaysQuery, List<Calendar>>
    {
        private readonly ICalendarRepository calendarRepository;

        public GetMonthHolidaysQueryHandler(
            ICalendarRepository calendarRepository)
        {
            this.calendarRepository = calendarRepository;
        }

        public async Task<List<Calendar>> Handle(GetMonthHolidaysQuery request, CancellationToken cancellationToken)
        {
            var response = await calendarRepository.GetMonthHolidays(request.Year, request.Month);

            return response;
        }
    }
}
