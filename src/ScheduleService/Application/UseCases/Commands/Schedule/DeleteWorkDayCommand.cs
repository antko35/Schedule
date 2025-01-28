namespace ScheduleService.Application.UseCases.Commands.Schedule
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Domain.Models;

    public record DeleteWorkDayCommand : IRequest<Schedule>
    {
        public string UserId { get; set; }
        public string DepartmentId { get; set; }
        public DateTime WorkDay { get; set; }
    }
}
