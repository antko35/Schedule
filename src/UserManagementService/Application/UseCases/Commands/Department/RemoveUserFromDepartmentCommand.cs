namespace UserManagementService.Application.UseCases.Commands.Department;

using MediatR;
using UserManagementService.Domain.Models;

public record RemoveUserFromDepartmentCommand(string userId, string departmentId)
    : IRequest<UserJob>;
