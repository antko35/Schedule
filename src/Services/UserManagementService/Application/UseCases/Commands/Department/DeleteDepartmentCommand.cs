using System.Windows.Input;
using MediatR;

namespace UserManagementService.Application.UseCases.Commands.Department;

public record DeleteDepartmentCommand(string DepartmentId)
    : IRequest;
