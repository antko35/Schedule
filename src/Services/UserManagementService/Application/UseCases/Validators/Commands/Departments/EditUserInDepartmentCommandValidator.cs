namespace UserManagementService.Application.UseCases.Validators.Commands.Departments;

using FluentValidation;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementService.Application.Extensions.Validation;
using UserManagementService.Application.UseCases.Commands.Department;
using UserManagementService.Application.UseCases.Validators.Commands.User;

public sealed class EditUserInDepartmentCommandValidator
    : AbstractValidator<EditUserInDepartmentCommand>
{
    public EditUserInDepartmentCommandValidator()
    {
        RuleFor(x => x.DepartmentId)
            .MustBeValidObjectId();

        RuleFor(x => x.UserId)
            .MustBeValidObjectId();

        RuleFor(x => x.Email)
            .EmailAddress();
    }
}
