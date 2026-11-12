namespace ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules;

using MediatR;
using ScheduleService.DataAccess.Database;
using ScheduleService.Application.Contracts;
using ScheduleService.Application.Mapping;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;

public sealed class CreateScheduleRulesCommandHandler
    : IRequestHandler<CreateScheduleRulesCommand, (ScheduleDto Schedule, ScheduleRulesDto ScheduleRules)>
{
    private const int DefaultTargetMinutes = 132 * 60;
    private const int DefaultShiftMinutes = 6 * 60 + 30;
    private readonly IScheduleGenerationRuleRepository ruleRepository;
    private readonly IScheduleRepository scheduleRepository;
    private readonly ScheduleDbContext dbContext;

    public CreateScheduleRulesCommandHandler(
        IScheduleGenerationRuleRepository ruleRepository,
        IScheduleRepository scheduleRepository,
        ScheduleDbContext dbContext)
    {
        this.ruleRepository = ruleRepository;
        this.scheduleRepository = scheduleRepository;
        this.dbContext = dbContext;
    }

    public async Task<(ScheduleDto Schedule, ScheduleRulesDto ScheduleRules)> Handle(
        CreateScheduleRulesCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var period = new DateOnly(now.Year, now.Month, 1);
        var schedule = await scheduleRepository.GetAsync(
            request.UserId,
            request.DepartmentId,
            period,
            cancellationToken);
        var rule = await ruleRepository.GetAsync(
            request.UserId,
            request.DepartmentId,
            period,
            cancellationToken);
        var hasChanges = false;

        if (schedule is null)
        {
            schedule = new MonthlySchedule(
                Guid.NewGuid().ToString("N"),
                request.UserId,
                request.DepartmentId,
                period,
                now);
            await scheduleRepository.AddAsync(schedule, cancellationToken);
            hasChanges = true;
        }

        if (rule is null)
        {
            rule = CreateDefaultRule(request.UserId, request.DepartmentId, period);
            await ruleRepository.AddAsync(rule, cancellationToken);
            hasChanges = true;
        }

        if (hasChanges)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return (schedule.ToDto(), rule.ToDto(schedule.Id));
    }

    private static ScheduleGenerationRule CreateDefaultRule(
        string userId,
        string departmentId,
        DateOnly period) =>
        new(
            Guid.NewGuid().ToString("N"),
            userId,
            departmentId,
            period,
            DefaultTargetMinutes,
            DefaultShiftMinutes,
            new TimeOnly(8, 0),
            new TimeOnly(14, 30),
            ShiftPattern.Manual);
}
