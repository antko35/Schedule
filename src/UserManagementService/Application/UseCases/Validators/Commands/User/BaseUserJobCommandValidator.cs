namespace UserManagementService.Application.UseCases.Validators.Commands.User
{
    using FluentValidation;
    using UserManagementService.Domain.Models;

    public abstract class BaseUserJobCommandValidator<T> : AbstractValidator<T> where T : UserJob
    {
        protected BaseUserJobCommandValidator()
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
