using MediatR;
using ScheduleService.Domain.Models;

namespace ScheduleService.Application.UseCases.Commands.ScheduleRules
{
    public class SetGenerationRulesCommand
        : IRequest<UserScheduleRules>
    {
        public string ScheduleRulesId { get; set; }

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
