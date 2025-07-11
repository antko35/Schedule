﻿namespace UserManagementService.Application.UseCases.CommandHandlers.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MediatR;
    using UserManagementService.Application.Extensions;
    using UserManagementService.Application.UseCases.Commands.User;
    using UserManagementService.Domain.Abstractions.IRepository;
    using UserManagementService.Domain.Models;

    public class UpdateUserInfoCommandHandler
        : IRequestHandler<UpdateUserInfoCommand, User>
    {
        private readonly IUserRepository userRepository;

        public UpdateUserInfoCommandHandler(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<User> Handle(UpdateUserInfoCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.UserId);

            user.EnsureExists("User not found");

            var newUser = new User
            {
                Id = request.UserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Patronymic = request.Patronymic,
                Gender = request.Gender,
                DateOfBirth = request.DateOfBirth,
            };

            await userRepository.UpdateAsync(newUser);

            return newUser;
        }
    }
}
