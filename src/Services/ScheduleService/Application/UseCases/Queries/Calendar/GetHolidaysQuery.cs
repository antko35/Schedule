namespace ScheduleService.Application.UseCases.Queries.Calendar
{
    using MediatR;
    using ScheduleService.Application.Contracts;

    public record GetHolidaysQuery
        : IRequest<List<CalendarDto>>;
}
