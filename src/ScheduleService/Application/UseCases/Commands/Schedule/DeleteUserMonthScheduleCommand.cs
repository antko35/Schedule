namespace ScheduleService.Application.UseCases.Commands.Schedule
{
    using MediatR;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class DeleteUserMonthScheduleCommand : IRequest<string>
    {
        public string UserId { get; set; }

        public string DepartmentId { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }
    }
}
