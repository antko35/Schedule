namespace UserManagementService.Application.UseCases.Queries.Department;

using MediatR;
using System.Collections.Generic;
using UserManagementService.Application.DTOs;

/// <summary>
/// Get full users info in departmernt
/// </summary>
/// <param name="DepartmentId"></param>
public record GetUsersByDepartmentQuery(string DepartmentId)
    : IRequest<IEnumerable<UserInfoInDepartment>>;
