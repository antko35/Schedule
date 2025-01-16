namespace UserManagementService.Application.UseCases.QueryHandlers.User
{
    using MediatR;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using UserManagementService.Application.DTOs;
    using UserManagementService.Application.UseCases.Queries.User;
    using UserManagementService.Domain.Abstractions.IRepository;

    public class GetFullUserInfoQueryHandler
        : IRequestHandler<GetFullUserInfoQuery, AllUserInfo>
    {
        private readonly IUserJobsRepository userJobsRepository;
        private readonly IUserRepository userRepository;

        public GetFullUserInfoQueryHandler(IUserRepository userRepository, IUserJobsRepository userJobsRepository)
        {
            this.userRepository = userRepository;
            this.userJobsRepository = userJobsRepository;
        }

        public async Task<AllUserInfo> Handle(GetFullUserInfoQuery request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.userId)
                ?? throw new KeyNotFoundException("User not found");

            var userJobs = await userJobsRepository.GetUserJobsByUserIdAsync(request.userId);

            Console.WriteLine(userJobs);
            Console.WriteLine(user.ToString());
            AllUserInfo response = new AllUserInfo()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Patronymic = user.Patronymic,
                Age = user.Age,
                Gender = user.Gender,
                Jobs = userJobs,
            };

            return response;
        }
    }
}
