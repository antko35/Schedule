namespace UserManagementService.Application.UseCases.Commands.Department;
using MediatR;
using UserManagementService.Application.DTOs;
using UserManagementService.Domain.Models;

public record EditUserInDepartmentCommand : IRequest<UserJob>
{
    public string DepartmentId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
