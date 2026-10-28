namespace ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules;

using MediatR;
using ScheduleService.DataAccess.Database;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using Domain.Abstractions;
using Domain.Models;

public sealed class AddingScheduleRulesForNextMonthCommandHandler
    : IRequestHandler<AddingScheduleRulesForNextMonthCommand>
{
    private readonly IScheduleGenerationRuleRepository ruleRepository;
    private readonly IScheduleRepository scheduleRepository;
    private readonly ScheduleDbContext dbContext;

    public AddingScheduleRulesForNextMonthCommandHandler(
        IScheduleGenerationRuleRepository ruleRepository,
        IScheduleRepository scheduleRepository,
        ScheduleDbContext dbContext)
    {
        this.ruleRepository = ruleRepository;
        this.scheduleRepository = scheduleRepository;
        this.dbContext = dbContext;
    }

    public async Task Handle(
        AddingScheduleRulesForNextMonthCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var currentPeriod = new DateOnly(now.Year, now.Month, 1);
        var nextPeriod = currentPeriod.AddMonths(1);
        var currentRules = await ruleRepository.GetAllByMonthAsync(currentPeriod, cancellationToken);
        if (currentRules.Count == 0)
        {
            throw new InvalidOperationException("No ScheduleRules");
        }

        var hasChanges = false;
        foreach (var currentRule in currentRules)
        {
            var schedule = await scheduleRepository.GetAsync(
                currentRule.UserId,
                currentRule.DepartmentId,
                nextPeriod,
                cancellationToken);
            var nextRule = await ruleRepository.GetAsync(
                currentRule.UserId,
                currentRule.DepartmentId,
                nextPeriod,
                cancellationToken);

            if (schedule is null)
            {
                schedule = new MonthlySchedule(
                    Guid.NewGuid().ToString("N"),
                    currentRule.UserId,
                    currentRule.DepartmentId,
                    nextPeriod,
                    now);
                await scheduleRepository.AddAsync(schedule, cancellationToken);
                hasChanges = true;
            }

            if (nextRule is null)
            {
                nextRule = new ScheduleGenerationRule(
                    Guid.NewGuid().ToString("N"),
                    currentRule.UserId,
                    currentRule.DepartmentId,
                    nextPeriod,
                    currentRule.TargetMinutes,
                    currentRule.MaxShiftMinutes,
                    currentRule.FirstShiftStart,
                    currentRule.SecondShiftStart,
                    currentRule.Pattern);

                await ruleRepository.AddAsync(nextRule, cancellationToken);

                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
