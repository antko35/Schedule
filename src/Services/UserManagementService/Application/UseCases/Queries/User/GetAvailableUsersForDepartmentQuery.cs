namespace UserManagementService.Application.UseCases.Queries.User;

using MediatR;
using UserManagementService.Application.DTOs;

public record GetAvailableUsersForDepartmentQuery(string DepartmentId)
    : IRequest<IEnumerable<ShortUserInfoDto>>;
