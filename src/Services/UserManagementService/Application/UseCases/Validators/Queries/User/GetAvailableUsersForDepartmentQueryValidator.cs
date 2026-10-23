namespace UserManagementService.Application.UseCases.Validators.Queries.User;

using FluentValidation;
using UserManagementService.Application.Extensions.Validation;
using UserManagementService.Application.UseCases.Queries.User;

public sealed class GetAvailableUsersForDepartmentQueryValidator
    : AbstractValidator<GetAvailableUsersForDepartmentQuery>
{
    public GetAvailableUsersForDepartmentQueryValidator()
    {
        RuleFor(query => query.DepartmentId)
            .NotEmpty()
            .MustBeValidObjectId();
    }
}
