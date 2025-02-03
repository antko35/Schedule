using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementService.Application.UseCases.Commands.Department;

namespace UserManagementService.Application.UseCases.Validators.Commands.Departments
{
    public sealed class RemoveUserFromDepartmentCommandValidator
        : AbstractValidator<RemoveUserFromDepartmentCommand>
    {
        public RemoveUserFromDepartmentCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.DepartmentId).NotEmpty();
        }
    }
}
