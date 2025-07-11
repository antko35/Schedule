﻿namespace UserManagementService.Application.UseCases.CommandHandlers.User
{
    using MediatR;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using UserManagementService.Application.Extensions;
    using UserManagementService.Application.UseCases.Commands.User;
    using UserManagementService.Domain.Abstractions.IRepository;

    public class DeleteUserCommandHandler
        : IRequestHandler<DeleteUserCommand>
    {
        private readonly IUserRepository userRepository;
        private readonly IUserJobsRepository userJobsRepository;

        public DeleteUserCommandHandler(IUserRepository userRepository,
                                        IUserJobsRepository userJobsRepository)
        {
            this.userRepository = userRepository;
            this.userJobsRepository = userJobsRepository;
        }

        public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.UserId);

            user.EnsureExists("User not found");

            await userRepository.RemoveAsync(request.UserId);

            await userJobsRepository.DeleteByUserId(request.UserId);
        }
    }
}
