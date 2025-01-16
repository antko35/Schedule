namespace UserManagementService.Application.UseCases.Queries.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MediatR;
    using UserManagementService.Application.DTOs;

    public record GetFullUserInfoQuery(string userId)
        : IRequest<AllUserInfo>
    {
    }
}
