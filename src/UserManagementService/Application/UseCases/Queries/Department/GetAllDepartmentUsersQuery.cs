namespace UserManagementService.Application.UseCases.Queries.Department;

using MediatR;
using System.Collections.Generic;
using UserManagementService.Domain.Models;

public record GetAllDepartmentUsersQuery : IRequest<IEnumerable<UserJob>>;
