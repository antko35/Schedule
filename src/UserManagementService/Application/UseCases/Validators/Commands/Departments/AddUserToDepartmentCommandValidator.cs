namespace UserManagementService.Application.UseCases.Validators.Commands.Departments
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using FluentValidation;
    using UserManagementService.Application.UseCases.Commands.Department;

    public sealed class AddUserToDepartmentCommandValidator
        : AbstractValidator<AddUserToDepartmentCommand>
    {
        public AddUserToDepartmentCommandValidator()
        {
            RuleFor(x => x.DepartmentId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Role).NotEmpty();
            RuleFor(x => x.Status).NotEmpty();
            RuleFor(x => x.Email).EmailAddress();
            RuleFor(x => x.PhoneNumber).NotEmpty();
        }
    }
}
