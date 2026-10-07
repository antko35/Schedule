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
