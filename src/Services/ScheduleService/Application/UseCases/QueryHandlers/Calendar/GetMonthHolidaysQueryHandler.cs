namespace ScheduleService.Application.UseCases.QueryHandlers.Calendar;

using MediatR;
using ScheduleService.Application.Contracts;
using ScheduleService.Application.Mapping;
using ScheduleService.Application.UseCases.Queries.Calendar;
using ScheduleService.Domain.Abstractions;

public sealed class GetMonthHolidaysQueryHandler
    : IRequestHandler<GetMonthHolidaysQuery, List<CalendarDto>>
{
    private readonly ICalendarRepository calendarRepository;

    public GetMonthHolidaysQueryHandler(ICalendarRepository calendarRepository)
    {
        this.calendarRepository = calendarRepository;
    }

    public async Task<List<CalendarDto>> Handle(
        GetMonthHolidaysQuery request,
        CancellationToken cancellationToken)
    {
        var period = new DateOnly(request.Year, request.Month, 1);
        var holidays = await calendarRepository.GetMonthHolidaysAsync(period, cancellationToken);
        return holidays.Select(item => item.ToDto()).ToList();
    }
}
