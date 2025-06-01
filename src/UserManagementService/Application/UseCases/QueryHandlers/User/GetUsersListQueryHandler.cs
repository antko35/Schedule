using AutoMapper;
using MediatR;
using UserManagementService.Application.DTOs;
using UserManagementService.Application.UseCases.Queries.User;
using UserManagementService.Domain.Abstractions.IRepository;

namespace UserManagementService.Application.UseCases.QueryHandlers.User;

public class GetUsersListQueryHandler : IRequestHandler<GetUsersListQuery, IEnumerable<ShortUserInfoDto>>
{
    private readonly IUserRepository userRepository;
    private readonly IUserJobsRepository userJobsRepository;
    private readonly IMapper _mapper;

    public GetUsersListQueryHandler(IUserRepository userRepository, IUserJobsRepository userJobsRepository, IMapper mapper)
    {
        this.userRepository = userRepository;
        this.userJobsRepository = userJobsRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ShortUserInfoDto>> Handle(GetUsersListQuery request, CancellationToken cancellationToken)
    {
        var userJobsFromDepartment = await userJobsRepository.GetUserJobsByDepartmentIdAsync(request.departmentId);

        var userIds = userJobsFromDepartment.Select(x => x.UserId).ToList();

        var users = await userRepository.GetByIdsAsync(userIds);

        var response = _mapper.Map<IEnumerable<ShortUserInfoDto>>(users);

        return response;
    }
}