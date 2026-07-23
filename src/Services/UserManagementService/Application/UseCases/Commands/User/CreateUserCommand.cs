namespace UserManagementService.Application.UseCases.Commands.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MediatR;
    using UserManagementService.Domain.Models;

    public record CreateUserCommand(
        string FirstName,
        string LastName,
        string Patronymic,
        string Gender,
        DateOnly DateOfBirth)
        : IRequest<User>;
}
