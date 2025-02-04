using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagementService.Application.Extensions
{
    public static class ValidationExtensions
    {
        private const string ObjectIdRegex = "^[a-fA-F0-9]{24}$";

        public static IRuleBuilderOptions<T, string> MustBeValidObjectId<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .Matches(ObjectIdRegex)
                .WithMessage("{PropertyName} must be a valid 24-character hex string.");
        }
    }
}
