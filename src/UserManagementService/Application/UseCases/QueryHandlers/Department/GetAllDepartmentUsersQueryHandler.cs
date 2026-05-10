namespace UserManagementService.Application.UseCases.QueryHandlers.Department;

using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserManagementService.Application.UseCases.Queries.Department;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;

public class GetAllDepartmentUsersQueryHandler : IRequestHandler<GetAllDepartmentUsersQuery, IEnumerable<UserJob>>
{
    private readonly IUserJobsRepository userJobsRepository;

    public GetAllDepartmentUsersQueryHandler(IUserJobsRepository userJobsRepository)
    {
        this.userJobsRepository = userJobsRepository;
    }

    public async Task<IEnumerable<UserJob>> Handle(GetAllDepartmentUsersQuery request, CancellationToken cancellationToken)
    {
        return await userJobsRepository.GetAllAsync();
    }
}
