using FluentValidation;
using ScheduleService.Application.UseCases.Queries.ScheduleRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.Validators.Queris.ScheduleRules
{
    public class GetUserRulesQueryValidator : AbstractValidator<GetUserRulesQuery>
    {
        public GetUserRulesQueryValidator()
        {
            RuleFor(x => x.Year)
                .InclusiveBetween(2000, 2100);

            RuleFor(x => x.Month)
                .InclusiveBetween(1, 12);
        }
    }
}
