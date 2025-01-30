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
        string userId,
        string departmentId,
        int year,
        int month)
        : IRequest<Schedule>
    {
        public string UserId { get; } = userId;

        public string DepartmentId { get; } = departmentId;

        public int Year { get; } = year;

        public int Month { get; } = month;
    }
}
