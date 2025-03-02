using MediatR;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Abstractions;

namespace ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules;

public class DeleteRulesAndScheduleCommandHandler : IRequestHandler<DeleteRulesAndScheduleCommand>
{
    private readonly IScheduleRepository scheduleRepository;
    private readonly IUserRuleRepository userRuleRepository;

    public DeleteRulesAndScheduleCommandHandler(
        IScheduleRepository scheduleRepository,
        IUserRuleRepository userRuleRepository)
    {
        this.scheduleRepository = scheduleRepository;
        this.userRuleRepository = userRuleRepository;
    }

    public async Task Handle(DeleteRulesAndScheduleCommand request, CancellationToken cancellationToken)
    {
        var allUserRules = await userRuleRepository.GetAllByIds(request.UserId, request.DepartmentId);

        var tasks = new List<Task>();
        foreach (var userRule in allUserRules)
        {
            var task = scheduleRepository.DeleteScheduleAsync(userRule.ScheduleId);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        await userRuleRepository.DeleteAll(request.UserId, request.DepartmentId);
    }
}