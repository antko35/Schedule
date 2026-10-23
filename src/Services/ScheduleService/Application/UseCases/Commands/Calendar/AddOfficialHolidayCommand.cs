namespace ScheduleService.Application.UseCases.Commands.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Domain.Models;

    public record AddOfficialHolidayCommand(DateOnly Holiday, DateOnly TransferDay)
        : IRequest<Calendar>;
}
