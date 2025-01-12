using MediatR;
using UserManagementService.Domain.Models;

namespace UserManagementService.Application.UseCases.Commands.Department;
public record AddUserToDepartmentCommand(UserJob userJob): IRequest<UserJob>
{
}
