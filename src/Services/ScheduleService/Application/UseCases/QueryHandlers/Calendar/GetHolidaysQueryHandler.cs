namespace ScheduleService.Application.UseCases.QueryHandlers.Calendar;
using MediatR;
using ScheduleService.Application.Contracts;
using ScheduleService.Application.Mapping;
using ScheduleService.Application.UseCases.Queries.Calendar;
using ScheduleService.Domain.Abstractions;
public sealed class GetHolidaysQueryHandler(ICalendarRepository calendarRepository)
    : IRequestHandler<GetHolidaysQuery, List<CalendarDto>>
{
    public async Task<List<CalendarDto>> Handle(GetHolidaysQuery request, CancellationToken cancellationToken) =>
        (await calendarRepository.GetAllAsync(cancellationToken)).OrderBy(item => item.HolidayDate).Select(item => item.ToDto()).ToList();
}
