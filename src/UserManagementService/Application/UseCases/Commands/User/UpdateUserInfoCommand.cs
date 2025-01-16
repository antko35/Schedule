namespace UserManagementService.Application.UseCases.Commands.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MediatR;
    using UserManagementService.Domain.Models;

    public record UpdateUserInfoCommand
        : IRequest<User>
    {
        required public string UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Patronymic { get; set; }
        public string? Gender { get; set; }
        public int Age { get; set; }
    }
}
