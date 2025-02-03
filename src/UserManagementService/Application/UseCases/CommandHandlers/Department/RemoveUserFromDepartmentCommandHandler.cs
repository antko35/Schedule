namespace UserManagementService.Application.UseCases.CommandHandlers.Department
{
    using MediatR;
    using UserManagementService.Application.UseCases.Commands.Department;
    using UserManagementService.Domain.Abstractions.IRepository;
    using UserManagementService.Domain.Models;

    public class RemoveUserFromDepartmentCommandHandler : IRequestHandler<RemoveUserFromDepartmentCommand, UserJob>
    {
        private readonly IUserJobsRepository userJobsRepository;

        public RemoveUserFromDepartmentCommandHandler(IUserJobsRepository userJobsRepository)
        {
            this.userJobsRepository = userJobsRepository;
        }

        public async Task<UserJob> Handle(RemoveUserFromDepartmentCommand request, CancellationToken cancellationToken)
        {
            var userJob = await userJobsRepository.GetUserJobAsync(request.UserId, request.DepartmentId)
                ?? throw new InvalidOperationException("User doesnt found in this department");

            await userJobsRepository.RemoveAsync(userJob.Id);
            return userJob;
        }
    }
}
