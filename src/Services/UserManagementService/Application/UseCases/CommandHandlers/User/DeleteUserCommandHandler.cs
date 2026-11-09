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
    private readonly IDataChanges changes;


    public DeleteUserCommandHandler(
        IUserRepository userRepository,
        IUserJobsRepository userJobsRepository,
        IUserEventPublisher userEventPublisher, IDataChanges changes)
    {
        this.changes = changes;
        this.userRepository = userRepository;
        this.userJobsRepository = userJobsRepository;
        this.userEventPublisher = userEventPublisher;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId);

        user.EnsureExists("User not found");

        var memberships = (await userJobsRepository.GetUserJobsByUserIdAsync(request.UserId)).ToList();
        await userJobsRepository.DeleteByUserId(request.UserId);
        await userRepository.RemoveAsync(request.UserId);
        await changes.SaveChangesAsync(cancellationToken);

        foreach (var membership in memberships)
        {
            await userEventPublisher.PublishUserDeleted(user.Id, membership.DepartmentId!);
        }
    }
}
