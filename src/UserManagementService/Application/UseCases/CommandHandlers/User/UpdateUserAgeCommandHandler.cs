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

        public UpdateUserAgeCommandHandler(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task Handle(UpdateUserAgeCommand request, CancellationToken cancellationToken)
        {
            var users = await userRepository.GetAllAsync();
            var today = DateOnly.FromDateTime(DateTime.Today);

            foreach (var user in users)
            {
                var calculatedAge = today.Year - user.DateOfBirth.Year;
                if (today < user.DateOfBirth.AddYears(calculatedAge))
                {
                    calculatedAge--;
                }

                user.Age = calculatedAge;
                await userRepository.UpdateAsync(user);
            }
        }
    }
}
