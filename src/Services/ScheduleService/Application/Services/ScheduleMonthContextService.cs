namespace ScheduleService.Application.Services;

using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;

public sealed class ScheduleMonthContextService(
    IScheduleGenerationRuleRepository ruleRepository,
    IScheduleRepository scheduleRepository)
{
    public async Task<(MonthlySchedule Schedule, ScheduleGenerationRule Rules, bool Created)> GetOrCreateAsync(
        string userId, string departmentId, int year, int month,
        CancellationToken cancellationToken = default)
    {
        var period = new DateOnly(year, month, 1);
        var rule = await ruleRepository.GetAsync(userId, departmentId, period, cancellationToken);
        var schedule = await scheduleRepository.GetAsync(userId, departmentId, period, cancellationToken);
        var created = rule is null;
        if (schedule is null)
        {
            schedule = new MonthlySchedule(Guid.NewGuid().ToString("N"), userId, departmentId, period, DateTime.UtcNow);
            await scheduleRepository.AddAsync(schedule, cancellationToken);
        }
        if (rule is null)
        {
            rule = new ScheduleGenerationRule(Guid.NewGuid().ToString("N"), userId, departmentId, period,
                132 * 60, 390, new TimeOnly(8, 0), new TimeOnly(14, 30), ShiftPattern.Manual);
            await ruleRepository.AddAsync(rule, cancellationToken);
        }
        return (schedule, rule, created);
    }
}
