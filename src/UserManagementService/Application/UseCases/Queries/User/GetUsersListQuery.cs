using MediatR;
using UserManagementService.Application.DTOs;

namespace UserManagementService.Application.UseCases.Queries.User;

public record GetUsersListQuery(string departmentId)
    : IRequest<IEnumerable<ShortUserInfoDto>>;