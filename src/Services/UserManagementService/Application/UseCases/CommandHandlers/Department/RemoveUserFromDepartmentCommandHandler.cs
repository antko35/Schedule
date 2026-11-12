using UserManagementService.Domain.Abstractions.IRabbitMq;

namespace UserManagementService.Application.UseCases.CommandHandlers.Department
{
    using MediatR;
    using UserManagementService.Application.Extensions;
    using UserManagementService.Application.UseCases.Commands.Department;
    using UserManagementService.Domain.Abstractions.IRepository;
    using UserManagementService.Domain.Models;

    public class RemoveUserFromDepartmentCommandHandler : IRequestHandler<RemoveUserFromDepartmentCommand, UserJob>
    {
        private readonly IUserJobsRepository userJobsRepository;
        private readonly IUserEventPublisher userEventPublisher;
        private readonly IDataChanges changes;


        public RemoveUserFromDepartmentCommandHandler(IUserJobsRepository userJobsRepository, IUserEventPublisher userEventPublisher, IDataChanges changes)
        {
            this.changes = changes;
            this.userJobsRepository = userJobsRepository;
            this.userEventPublisher = userEventPublisher;
        }

        public async Task<UserJob> Handle(RemoveUserFromDepartmentCommand request, CancellationToken cancellationToken)
        {
            var userJob = await userJobsRepository.GetUserJobAsync(request.UserId, request.DepartmentId);

            userJob.EnsureExists("User not found in this department");

            await userJobsRepository.RemoveAsync(userJob.Id);
            await changes.SaveChangesAsync(cancellationToken);

            await userEventPublisher.PublishUserDeleted(userJob.UserId, userJob.DepartmentId!);

            return userJob;
        }
    }
}
