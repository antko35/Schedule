namespace ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules;

using MediatR;
using ScheduleService.DataAccess.Database;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.Domain.Abstractions;

public sealed class DeleteRulesAndScheduleCommandHandler
    : IRequestHandler<DeleteRulesAndScheduleCommand>
{
    private readonly IScheduleRepository scheduleRepository;
    private readonly IScheduleGenerationRuleRepository ruleRepository;
    private readonly ScheduleDbContext dbContext;

    public DeleteRulesAndScheduleCommandHandler(
        IScheduleRepository scheduleRepository,
        IScheduleGenerationRuleRepository ruleRepository,
        ScheduleDbContext dbContext)
    {
        this.scheduleRepository = scheduleRepository;
        this.ruleRepository = ruleRepository;
        this.dbContext = dbContext;
    }

    public async Task Handle(
        DeleteRulesAndScheduleCommand request,
        CancellationToken cancellationToken)
    {
        var rules = await ruleRepository.GetAllByUserDepartmentAsync(
            request.UserId,
            request.DepartmentId,
            cancellationToken);

        foreach (var rule in rules)
        {
            var schedule = await scheduleRepository.GetAsync(
                rule.UserId,
                rule.DepartmentId,
                rule.Period,
                cancellationToken);
            if (schedule is not null)
            {
                scheduleRepository.Remove(schedule);
            }
        }

        ruleRepository.RemoveRange(rules);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
