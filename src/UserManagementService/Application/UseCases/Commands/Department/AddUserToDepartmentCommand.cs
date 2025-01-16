namespace UserManagementService.Application.UseCases.Commands.Department;

using MediatR;
using UserManagementService.Domain.Models;
public record AddUserToDepartmentCommand()
    : IRequest<UserJob>
{
    required public string UserId { get; set; }
    required public string DepartmentId { get; set; }
    required public string Role { get; set; }
    required public string Status { get; set; }
    required public string Email { get; set; }
    required public string PhoneNumber { get; set; }
}
