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
        public static IRuleBuilderOptions<T, string> MustBeValidObjectId<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .Must(BeAValidObjectId)
                .WithMessage("{PropertyName} must be a valid 24-character hex string.");
        }

        private static bool BeAValidObjectId(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length != 24)
            {
                return false;
            }

            foreach (char c in value)
            {
                if (!IsHexDigit(c))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsHexDigit(char c)
        {
            return c >= '0' && c <= '9' ||
                   c >= 'a' && c <= 'f' ||
                   c >= 'A' && c <= 'F';
        }
    }
}
