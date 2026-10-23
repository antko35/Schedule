namespace ScheduleService.Application.UseCases.QueryHandlers.Calendar
{
    using MediatR;
    using ScheduleService.Application.UseCases.Queries.Calendar;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class GetHolidaysQueryHandler
        : IRequestHandler<GetHolidaysQuery, List<Calendar>>
    {
        private readonly ICalendarRepository calendarRepository;

        public GetHolidaysQueryHandler(ICalendarRepository calendarRepository)
        {
            this.calendarRepository = calendarRepository;
        }

        public async Task<List<Calendar>> Handle(
            GetHolidaysQuery request,
            CancellationToken cancellationToken)
        {
            var holidays = await calendarRepository.GetAllAsync();

            return holidays
                .OrderBy(x => x.HolidayDate)
                .ToList();
        }
    }
}
