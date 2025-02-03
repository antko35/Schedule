using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementService.Application.UseCases.Commands.User;

namespace UserManagementService.Application.UseCases.Validators.Commands.User
{
    public sealed class CreateUserCommandValidator
        : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty();
            RuleFor(x => x.LastName)
                .NotEmpty();
            RuleFor(x => x.Gender)
                .NotEmpty();
            RuleFor(x => x.DateOfBirth)
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Date of birth cannot be in the future.");
        }
    }
}
