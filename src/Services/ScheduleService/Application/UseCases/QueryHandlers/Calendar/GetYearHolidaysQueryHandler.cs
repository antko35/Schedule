namespace ScheduleService.Application.UseCases.QueryHandlers.Calendar
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain.Abstractions;
    using Domain.Models;
    using MediatR;
    using ScheduleService.Application.UseCases.Queries.Calendar;

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
