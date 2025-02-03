namespace ScheduleService.Application.UseCases.Commands.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Domain.Models;

    public record GetMonthHolidaysCommand(int Year, int Month)
        : IRequest<List<Calendar>>
    {
    }
}
