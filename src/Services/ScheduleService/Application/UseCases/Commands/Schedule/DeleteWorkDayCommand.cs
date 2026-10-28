namespace ScheduleService.Application.UseCases.Commands.Schedule
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Application.Contracts;

    public record DeleteWorkDayCommand(
        string UserId,
        string DepartmentId,
        DateTime WorkDay)
        : IRequest<ScheduleDto>;
}
