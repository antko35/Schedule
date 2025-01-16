namespace UserManagementService.Application.UseCases.Commands.Department;

using MediatR;
using UserManagementService.Domain.Models;

public record CreateDepartmentCommand
    : IRequest<Department>
{
    public string DeartmentName { get; set; } = string.Empty;
    public string ClinicId { get; set; } = string.Empty;
}
