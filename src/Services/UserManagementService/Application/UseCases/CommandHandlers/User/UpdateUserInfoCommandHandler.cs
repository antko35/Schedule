namespace UserManagementService.Application.UseCases.CommandHandlers.User
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
        private readonly IDataChanges changes;


        public UpdateUserInfoCommandHandler(IUserRepository userRepository, IDataChanges changes)
        {
            this.changes = changes;
            this.userRepository = userRepository;
        }

        public async Task<User> Handle(UpdateUserInfoCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.UserId);

            user.EnsureExists("User not found");

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Patronymic = request.Patronymic;
            user.Gender = request.Gender;
            user.DateOfBirth = request.DateOfBirth;
            await userRepository.UpdateAsync(user);
            await changes.SaveChangesAsync(cancellationToken);
            return user;
        }
    }
}
