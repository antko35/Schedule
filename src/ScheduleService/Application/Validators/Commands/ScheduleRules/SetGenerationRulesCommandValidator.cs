using FluentValidation;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.Validators.Commands.ScheduleRules
{
    public class SetGenerationRulesCommandValidator 
        : AbstractValidator<SetGenerationRulesCommand>
    {
        public SetGenerationRulesCommandValidator()
        {
            RuleFor(x => x.HoursPerMonth)
                .GreaterThan(0).WithMessage("HoursPerMonth must be greater than 0.");

            RuleFor(x => x.MaxHoursPerDay)
                .GreaterThan(0).WithMessage("MaxHoursPerDay must be greater than 0.");

            RuleFor(x => x)
                .Must(x =>
                {
                    var flags = new bool?[]
                    {
                        x.EvenDOW,
                        x.UnEvenDOW,
                        x.EvenDOM,
                        x.UnEvenDOM,
                        x.OnlyFirstShift,
                        x.OnlySecondShift,
                    };

                    return flags.Count(f => f.HasValue && f.Value) <= 1;
                })
                .WithMessage("Only one of EvenDOW, UnEvenDOW, EvenDOM, UnEvenDOM, OnlyFirstShift, OnlySecondShift can be true.");

            RuleFor(x => x)
                .Must(x =>
                {
                    var workDaysCont = x.HoursPerMonth / x.MaxHoursPerDay;
                    return workDaysCont >= 15 && workDaysCont <= 23;
                })
                .WithMessage("Invalid hours per month or work day length.")
                .When(x => x.HoursPerMonth.HasValue && x.MaxHoursPerDay.HasValue);
        }
    }
}
