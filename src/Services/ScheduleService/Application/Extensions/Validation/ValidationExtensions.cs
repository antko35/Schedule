using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.Extensions.Validation
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, string> MustBeValidEntityId<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty()
                .MaximumLength(128)
                .WithMessage("{PropertyName} must be a non-empty identifier up to 128 characters.");
        }
    }
}
