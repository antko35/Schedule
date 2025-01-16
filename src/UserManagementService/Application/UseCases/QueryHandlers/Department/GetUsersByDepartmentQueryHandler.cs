namespace UserManagementService.Application.UseCases.QueryHandlers.Department;

using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UserManagementService.Application.DTOs;
using UserManagementService.Application.UseCases.Queries.Department;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;

public class GetUsersByDepartmentQueryHandler : IRequestHandler<GetUsersByDepartmentQuery, IEnumerable<FullUserInfo>>
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

    public async Task<IEnumerable<FullUserInfo>> Handle(GetUsersByDepartmentQuery request, CancellationToken cancellationToken)
    {
        var department = await departmentRepository.GetByIdAsync(request.departmentId)
            ?? throw new KeyNotFoundException("Department doesnt exist");

        var userJobs = await userJobsRepository.GetUserJobsByDepartmentIdAsync(request.departmentId);

        List<FullUserInfo> users = new List<FullUserInfo>();
        foreach (var userJob in userJobs)
        {
            var user = await userRepository.GetByIdAsync(userJob.UserId);

            FullUserInfo fullUserInfo = new FullUserInfo(user, userJob);
            users.Add(fullUserInfo);
        }

        return users;
    }
}
