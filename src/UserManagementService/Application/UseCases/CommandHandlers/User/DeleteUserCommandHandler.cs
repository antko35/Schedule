using MediatR;
using UserManagementService.Application.Extensions;
using UserManagementService.Application.UseCases.Commands.User;
using UserManagementService.Domain.Abstractions.IRabbitMq;
using UserManagementService.Domain.Abstractions.IRepository;

namespace UserManagementService.Application.UseCases.CommandHandlers.User;

public class DeleteUserCommandHandler
    : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository userRepository;
    private readonly IUserJobsRepository userJobsRepository;
    private readonly IUserEventPublisher userEventPublisher;

    public DeleteUserCommandHandler(
        IUserRepository userRepository,
        IUserJobsRepository userJobsRepository,
        IUserEventPublisher userEventPublisher)
    {
        this.userRepository = userRepository;
        this.userJobsRepository = userJobsRepository;
        this.userEventPublisher = userEventPublisher;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId);

        user.EnsureExists("User not found");

        await userRepository.RemoveAsync(request.UserId);

        await DeleteUserSchedule(request.UserId);

        await userJobsRepository.DeleteByUserId(request.UserId);
    }

    private async Task DeleteUserSchedule(string userId)
    {
        var userJobs = await userJobsRepository.GetUserJobsByUserIdAsync(userId);

        var departments = userJobs.Select(userJob => userJob.DepartmentId).ToList();

        foreach (var id in departments)
        {
            await userEventPublisher.PublishUserDeleted(userId, id);
        }
    }
}