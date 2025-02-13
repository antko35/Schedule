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
                    bool isDOWSet = x.EvenDOW.HasValue || x.UnEvenDOW.HasValue;
                    bool isDOMSet = x.EvenDOM.HasValue || x.UnEvenDOM.HasValue;

                    if (isDOWSet && isDOMSet)
                    {
                        return false;
                    }

                    return true;
                })
                .WithMessage("You must specify either evenDOW and unEvenDOW, or evenDOM and unEvenDOM, but not both.");

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
