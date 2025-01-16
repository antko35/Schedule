namespace UserManagementService.Application.UseCases.Validators.Commands.User
{
    using FluentValidation;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UserManagementService.Application.UseCases.Commands.User;

    public sealed class UpdateUserInfoCommandValidator
        : AbstractValidator<UpdateUserInfoCommand>
    {
        public UpdateUserInfoCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}
