using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementService.Application.Extensions.Validation;
using UserManagementService.Application.UseCases.Queries.Department;

namespace UserManagementService.Application.UseCases.Validators.Queries.Departments
{
    public sealed class GetUsersByDepartmentQueryValidator
        : AbstractValidator<GetUsersByDepartmentQuery>
    {
        public GetUsersByDepartmentQueryValidator()
        {
            RuleFor(x => x.DepartmentId)
                .MustBeValidObjectId();
        }
    }
}
