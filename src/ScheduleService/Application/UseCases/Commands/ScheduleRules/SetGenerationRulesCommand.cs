using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.UseCases.Commands.ScheduleRules
{
    public class SetGenerationRulesCommand
        : IRequest
    {
        public string ScheduleRulesId { get; set; }

        public float? HoursPerMonth { get; set; }

        public float? MaxHoursPerDay { get; set; }

        public TimeOnly StartWorkDayTime { get; set; } = new TimeOnly(8, 0, 0);

        public bool? EvenDOW { get; set; }

        public bool? UnEvenDOW { get; set; }

        public bool? EvenDOM { get; set; }

        public bool? UnEvenDOM { get; set; }
    }
}
