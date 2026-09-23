namespace ScheduleService.Application.UseCases.Queries.Calendar
{
    using System.Collections.Generic;
    using MediatR;
    using Domain.Models;

    public record GetYearHolidaysQuery(int Year)
        : IRequest<List<Calendar>>;
}
