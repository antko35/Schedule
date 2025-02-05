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
                .MustBeValidObjectId();

            RuleFor(x => x.DepartmentId)
                .MustBeValidObjectId();

            RuleFor(x => x.Email)
                .EmailAddress();
        }
    }
}
