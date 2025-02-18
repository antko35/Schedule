namespace UserManagementService.Application.UseCases.Commands.Department;

using MediatR;
using UserManagementService.Domain.Models;
public record AddUserToDepartmentCommand(
    string UserId,
    string DepartmentId,
    string Role,
    string Status,
    string Email,
    string PhoneNumber)
    : IRequest<UserJob>;
