namespace UserManagementService.Application.UseCases.Commands.Department;
using MediatR;
using UserManagementService.Application.DTOs;
using UserManagementService.Domain.Models;

public record EditUserInDepartmentCommand(
    string DepartmentId,
    string UserId,
    string Role,
    string Status,
    string Email,
    string PhoneNumber) : IRequest<UserJob>;
