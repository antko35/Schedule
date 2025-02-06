using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.UseCases.Commands.ScheduleRules
{
    public record CreateScheduleRulesCommand(
        string UserId,
        string DepartmentId,
        int Month,
        int Year) : IRequest;
}
