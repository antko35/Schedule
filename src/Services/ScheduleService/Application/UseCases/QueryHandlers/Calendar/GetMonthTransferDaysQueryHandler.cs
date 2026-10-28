namespace ScheduleService.Application.UseCases.QueryHandlers.Calendar;

using MediatR;
using ScheduleService.Application.Contracts;
using ScheduleService.Application.Mapping;
using ScheduleService.Application.UseCases.Queries.Calendar;
using ScheduleService.Domain.Abstractions;

public sealed class GetMonthTransferDaysQueryHandler
    : IRequestHandler<GetMonthTransferDaysQuery, List<CalendarDto>>
{
    private readonly ICalendarRepository calendarRepository;

    public GetMonthTransferDaysQueryHandler(ICalendarRepository calendarRepository)
    {
        this.calendarRepository = calendarRepository;
    }

    public async Task<List<CalendarDto>> Handle(
        GetMonthTransferDaysQuery request,
        CancellationToken cancellationToken)
    {
        var period = new DateOnly(request.Year, request.Month, 1);
        var transferDays = await calendarRepository.GetMonthTransferDaysAsync(period, cancellationToken);
        return transferDays.Select(item => item.ToDto()).ToList();
    }
}
