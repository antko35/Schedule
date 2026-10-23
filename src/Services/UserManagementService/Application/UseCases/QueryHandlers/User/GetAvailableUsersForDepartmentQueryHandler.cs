namespace UserManagementService.Application.UseCases.QueryHandlers.User;

using AutoMapper;
using MediatR;
using UserManagementService.Application.DTOs;
using UserManagementService.Application.Extensions;
using UserManagementService.Application.UseCases.Queries.User;
using UserManagementService.Domain.Abstractions.IRepository;

public class GetAvailableUsersForDepartmentQueryHandler
    : IRequestHandler<GetAvailableUsersForDepartmentQuery, IEnumerable<ShortUserInfoDto>>
{
    private readonly IDepartmentRepository departmentRepository;
    private readonly IUserJobsRepository userJobsRepository;
    private readonly IUserRepository userRepository;
    private readonly IMapper mapper;

    public GetAvailableUsersForDepartmentQueryHandler(
        IDepartmentRepository departmentRepository,
        IUserJobsRepository userJobsRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        this.departmentRepository = departmentRepository;
        this.userJobsRepository = userJobsRepository;
        this.userRepository = userRepository;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<ShortUserInfoDto>> Handle(
        GetAvailableUsersForDepartmentQuery request,
        CancellationToken cancellationToken)
    {
        var department = await departmentRepository.GetByIdAsync(request.DepartmentId);

        department.EnsureExists("Department not found");

        var existingUserJobs = await userJobsRepository.GetUserJobsByDepartmentIdAsync(request.DepartmentId);
        var existingUserIds = existingUserJobs
            .Select(userJob => userJob.UserId)
            .Where(userId => !string.IsNullOrWhiteSpace(userId))
            .Distinct()
            .ToList();
        var availableUsers = await userRepository.GetAvailableUsersForDepartmentAsync(
            existingUserIds,
            cancellationToken);

        return mapper.Map<IEnumerable<ShortUserInfoDto>>(availableUsers);
    }
}
