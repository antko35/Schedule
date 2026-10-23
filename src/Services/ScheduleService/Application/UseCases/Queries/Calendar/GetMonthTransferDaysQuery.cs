namespace ScheduleService.Application.UseCases.Queries.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Domain.Models;

    public record GetMonthTransferDaysQuery(int Year, int Month)
        : IRequest<List<Calendar>>
    {
    }
}
