namespace ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules;

using MediatR;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;

public sealed class CreateRulesForSpecificMonthCommandHandler : IRequestHandler<CreateRulesForSpecificMonthCommand>
{
    // This endpoint was already unimplemented on main; month creation is handled by PUT rules.
    public Task Handle(CreateRulesForSpecificMonthCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
