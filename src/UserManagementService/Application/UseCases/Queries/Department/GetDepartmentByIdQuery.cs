namespace UserManagementService.Application.UseCases.Queries.Department;
using MediatR;
using UserManagementService.Domain.Models;
public record GetDepartmentByIdQuery(string Id)
    : IRequest<Department>;
