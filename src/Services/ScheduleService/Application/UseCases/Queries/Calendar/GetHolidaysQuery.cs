namespace ScheduleService.Application.UseCases.Queries.Calendar
{
    using MediatR;
    using ScheduleService.Domain.Models;

    public record GetHolidaysQuery
        : IRequest<List<Calendar>>;
}
