using AutoMapper;
using MediatR;
using UserManagementService.Application.DTOs;
using UserManagementService.Application.UseCases.Queries.User;
using UserManagementService.Domain.Abstractions.IRepository;

namespace UserManagementService.Application.UseCases.QueryHandlers.User;

public class GetUsersListQueryHandler : IRequestHandler<GetUsersListQuery, IEnumerable<ShortUserInfoDto>>
{
    private readonly IUserRepository userRepository;
    private readonly IMapper _mapper;

    public GetUsersListQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        this.userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ShortUserInfoDto>> Handle(GetUsersListQuery request, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync();

        var response = _mapper.Map<IEnumerable<ShortUserInfoDto>>(users);

        return response;
    }
}