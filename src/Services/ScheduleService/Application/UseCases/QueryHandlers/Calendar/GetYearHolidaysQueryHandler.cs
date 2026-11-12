namespace ScheduleService.Application.UseCases.QueryHandlers.Calendar;

using MediatR;
using ScheduleService.Application.Contracts;
using ScheduleService.Application.Mapping;
using ScheduleService.Application.UseCases.Queries.Calendar;
using ScheduleService.Domain.Abstractions;

public sealed class GetYearHolidaysQueryHandler
    : IRequestHandler<GetYearHolidaysQuery, List<CalendarDto>>
{
    private readonly ICalendarRepository calendarRepository;

    public GetYearHolidaysQueryHandler(ICalendarRepository calendarRepository)
    {
        this.calendarRepository = calendarRepository;
    }

    public async Task<List<CalendarDto>> Handle(
        GetYearHolidaysQuery request,
        CancellationToken cancellationToken)
    {
        var holidays = await calendarRepository.GetYearHolidaysAsync(request.Year, cancellationToken);
        return holidays.Select(item => item.ToDto()).ToList();
    }
}
