using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.UseCases.Commands.ScheduleRules
{
    public class SetGenerationRulesCommand
        : IRequest<ScheduleService.Application.Contracts.ScheduleRulesDto>
    {
        public string ScheduleRulesId { get; set; } = string.Empty;

        public float? HoursPerMonth { get; set; }

        public float? MaxHoursPerDay { get; set; }

        public TimeOnly? StartWorkDayTime { get; set; }

        public bool? OnlyFirstShift { get; set; }

        public bool? OnlySecondShift { get; set; }

        public bool? EvenDOW { get; set; }

        public bool? UnEvenDOW { get; set; }

        public bool? EvenDOM { get; set; }

        public bool? UnEvenDOM { get; set; }
    }
}
