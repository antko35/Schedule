namespace UserManagementService.Application.UseCases.Validators.Commands.User
{
    using FluentValidation;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UserManagementService.Application.Extensions.Validation;
    using UserManagementService.Application.UseCases.Commands.User;

    public sealed class UpdateUserInfoCommandValidator
        : AbstractValidator<UpdateUserInfoCommand>
    {
        public UpdateUserInfoCommandValidator()
        {
            RuleFor(x => x.UserId)
                .MustBeValidObjectId();

            RuleFor(x => x.DateOfBirth)
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Date of birth cannot be in the future.");
        }
    }
}
