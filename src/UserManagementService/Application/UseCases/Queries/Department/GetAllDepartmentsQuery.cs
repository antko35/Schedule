namespace UserManagementService.Application.UseCases.Queries.Department;
using MediatR;
using UserManagementService.Domain.Models;
public record GetAllDepartmentsQuery : IRequest<IEnumerable<Department>>;
