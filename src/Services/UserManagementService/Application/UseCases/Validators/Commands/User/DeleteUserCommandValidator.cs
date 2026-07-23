using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementService.Application.Extensions.Validation;
using UserManagementService.Application.UseCases.Commands.User;

namespace UserManagementService.Application.UseCases.Validators.Commands.User
{
    public class DeleteUserCommandValidator
        : AbstractValidator<DeleteUserCommand>
    {
        public DeleteUserCommandValidator()
        {
            RuleFor(x => x.UserId)
                .MustBeValidObjectId();
        }
    }
}
