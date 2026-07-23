using MediatR;
using ScheduleService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.UseCases.Queries.ScheduleRules
{
    public record GetUserRulesQuery(
        string UserId,
        string DepartmentId,
        int Month,
        int Year)
        : IRequest<UserScheduleRules>
    {
    }
}
