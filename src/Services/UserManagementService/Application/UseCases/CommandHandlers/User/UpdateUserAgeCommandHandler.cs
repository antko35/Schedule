namespace UserManagementService.Application.UseCases.CommandHandlers.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MediatR;
    using UserManagementService.Application.UseCases.Commands.User;
    using UserManagementService.Domain.Abstractions.IRepository;

    public class UpdateUserAgeCommandHandler
        : IRequestHandler<UpdateUserAgeCommand>
    {
        private readonly IUserRepository userRepository;
        private readonly IDataChanges changes;


        public UpdateUserAgeCommandHandler(IUserRepository userRepository, IDataChanges changes)
        {
            this.changes = changes;
            this.userRepository = userRepository;
        }

        public async Task Handle(UpdateUserAgeCommand request, CancellationToken cancellationToken)
        {
            var users = await userRepository.GetAllAsync();

            foreach (var user in users)
            {
                user.CalculateAge();

                await userRepository.UpdateAsync(user);
            }

            await changes.SaveChangesAsync(cancellationToken);
        }
    }
}
