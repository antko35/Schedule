namespace ScheduleService.Application.UseCases.Commands.Schedule
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Domain.Models;

    public record GetUserMonthScheduleCommand(
        string UserId,
        string DepartmentId,
        int Year,
        int Month)
        : IRequest<Schedule>
    { }
}
