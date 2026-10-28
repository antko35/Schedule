using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScheduleService.Application.Contracts;

namespace ScheduleService.Application.UseCases.Commands.ScheduleRules
{
    public record CreateScheduleRulesCommand(
        string UserId,
        string DepartmentId) : IRequest<(ScheduleDto Schedule, ScheduleRulesDto ScheduleRules)>;
}
