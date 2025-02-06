namespace ScheduleService.Application.UseCases.Commands.Schedule
{
    using MediatR;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public record DeleteUserMonthScheduleCommand(string UserId, string DepartmentId, int Month, int Year)
        : IRequest<string>;
}
