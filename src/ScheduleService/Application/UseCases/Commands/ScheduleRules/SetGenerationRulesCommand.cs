using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.UseCases.Commands.ScheduleRules
{
    public sealed record SetGenerationRulesCommand(
        string Id,
        float HoursPerMonth,
        float MaxHoursPerDay,
        bool EvenDOW,
        bool UnEvenDOW,
        bool EvenDOM,
        bool UnEvenDOM)
        : IRequest;
}
