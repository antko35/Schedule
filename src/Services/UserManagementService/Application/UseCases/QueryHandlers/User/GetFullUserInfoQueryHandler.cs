namespace UserManagementService.Application.UseCases.QueryHandlers.User
{
    using MediatR;
    using System.Threading;
    using System.Threading.Tasks;
    using UserManagementService.Application.DTOs;
    using UserManagementService.Application.Extensions;
    using UserManagementService.Application.UseCases.Queries.User;
    using UserManagementService.Domain.Abstractions.IRepository;

    public class GetFullUserInfoQueryHandler
        : IRequestHandler<GetFullUserInfoQuery, FullUserInfo>
    {
        private readonly IUserJobsRepository userJobsRepository;
        private readonly IUserRepository userRepository;

        public GetFullUserInfoQueryHandler(IUserRepository userRepository, IUserJobsRepository userJobsRepository)
        {
            this.userRepository = userRepository;
            this.userJobsRepository = userJobsRepository;
        }

        public async Task<FullUserInfo> Handle(GetFullUserInfoQuery request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.UserId);

            user.EnsureExists("User not found");

            var userJobs = await userJobsRepository.GetUserJobsByUserIdAsync(request.UserId);

            FullUserInfo response = new FullUserInfo()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Patronymic = user.Patronymic,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Jobs = userJobs,
            };

            return response;
        }
    }
}
