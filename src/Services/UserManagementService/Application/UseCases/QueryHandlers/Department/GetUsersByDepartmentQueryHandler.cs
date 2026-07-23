namespace UserManagementService.Application.UseCases.QueryHandlers.Department;

using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UserManagementService.Application.DTOs;
using UserManagementService.Application.Extensions;
using UserManagementService.Application.UseCases.Queries.Department;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;

public class GetUsersByDepartmentQueryHandler : IRequestHandler<GetUsersByDepartmentQuery, IEnumerable<UserInfoInDepartment>>
{
    private readonly IDepartmentRepository departmentRepository;
    private readonly IUserRepository userRepository;
    private readonly IUserJobsRepository userJobsRepository;

    public GetUsersByDepartmentQueryHandler(
        IDepartmentRepository departmentRepository,
        IUserRepository userRepository,
        IUserJobsRepository userJobsRepository)
    {
        this.departmentRepository = departmentRepository;
        this.userRepository = userRepository;
        this.userJobsRepository = userJobsRepository;
    }

    public async Task<IEnumerable<UserInfoInDepartment>> Handle(GetUsersByDepartmentQuery request, CancellationToken cancellationToken)
    {
        var department = await departmentRepository.GetByIdAsync(request.DepartmentId);

        department.EnsureExists("Department doesnt exist");

        var userJobs = await userJobsRepository.GetUserJobsByDepartmentIdAsync(request.DepartmentId);

        List<UserInfoInDepartment> users = new List<UserInfoInDepartment>();
        foreach (var userJob in userJobs)
        {
            var user = await userRepository.GetByIdAsync(userJob.UserId);

            UserInfoInDepartment fullUserInfo = new UserInfoInDepartment(user, userJob);
            users.Add(fullUserInfo);
        }

        return users;
    }
}
