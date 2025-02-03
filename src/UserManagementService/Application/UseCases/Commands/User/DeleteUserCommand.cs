namespace UserManagementService.Application.UseCases.Commands.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MediatR;

    /// <summary>
    /// Delete User and his UserJods
    /// </summary>
    /// <param name="UserId"></param>
    public record DeleteUserCommand(string UserId)
        : IRequest;
}
