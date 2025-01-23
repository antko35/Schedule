using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.UseCases.Commands.ScheduleRules
{
    public record CreateScheduleRulesCommand : IRequest
    {
        public string UserId { get; set; }
        public string DepartmentId { get; set; }
        public string Month { get; set; }
    }
}
