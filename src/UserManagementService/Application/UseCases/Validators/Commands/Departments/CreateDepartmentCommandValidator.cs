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

    public sealed class CreateDepartmentCommandValidator
        : AbstractValidator<CreateDepartmentCommand>
    {
        public CreateDepartmentCommandValidator()
        {
            RuleFor(x => x.ClinicId)
                .MustBeValidObjectId().When(x => !string.IsNullOrEmpty(x.ClinicId));
        }
    }
}
