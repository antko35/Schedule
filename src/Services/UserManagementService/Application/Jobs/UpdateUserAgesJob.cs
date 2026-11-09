namespace UserManagementService.Application.Jobs;

using MediatR;
using UserManagementService.Application.UseCases.Commands.User;

public class UpdateUserAgesJob(IMediator mediator)
{
    public Task ExecuteAsync(CancellationToken cancellationToken) =>
        mediator.Send(new UpdateUserAgeCommand(), cancellationToken);
}
