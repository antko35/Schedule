namespace UserManagementService.Application.UseCases.Validators.Commands.Departments
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using FluentValidation;
    using UserManagementService.Application.Extensions.Validation;
    using UserManagementService.Application.UseCases.Commands.Department;

    public sealed class AddUserToDepartmentCommandValidator
        : AbstractValidator<AddUserToDepartmentCommand>
    {
        public AddUserToDepartmentCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .MustBeValidObjectId();

            RuleFor(x => x.DepartmentId)
                .NotEmpty()
                .MustBeValidObjectId();

            RuleFor(x => x.Role)
                .NotEmpty();

            RuleFor(x => x.Status)
                .NotEmpty();

            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.PhoneNumber)
                .Matches(@"\A\+375-[0-9]{2}-[0-9]{7}\z")
                .WithMessage("PhoneNumber must match the +375-XX-XXXXXXX format.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        }
    }
}
