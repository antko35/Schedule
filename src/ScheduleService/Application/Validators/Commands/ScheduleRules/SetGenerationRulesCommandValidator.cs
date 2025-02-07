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
        }
    }
}
