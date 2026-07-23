namespace ScheduleService.Application.UseCases.QueryHandlers.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Application.UseCases.Queries.Calendar;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class GetYearHolidaysQueryHandler
        : IRequestHandler<GetYearHolidaysQuery, List<Calendar>>
    {
        private readonly ICalendarRepository calendarRepository;

        public GetYearHolidaysQueryHandler(ICalendarRepository calendarRepository)
        {
            this.calendarRepository = calendarRepository;
        }

        public async Task<List<Calendar>> Handle(GetYearHolidaysQuery request, CancellationToken cancellationToken)
        {
            var holidays = await calendarRepository.GetYearHolidays(request.Year);

            return holidays;
        }
    }
}
