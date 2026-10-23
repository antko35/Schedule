using MongoDB.Bson;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;

namespace ScheduleService.Application.Services;

public class ScheduleMonthContextService
{
    private readonly IUserRuleRepository userRuleRepository;
    private readonly IScheduleRepository scheduleRepository;

    public ScheduleMonthContextService(
        IUserRuleRepository userRuleRepository,
        IScheduleRepository scheduleRepository)
    {
        this.userRuleRepository = userRuleRepository;
        this.scheduleRepository = scheduleRepository;
    }

    public async Task<(Schedule Schedule, UserScheduleRules Rules, bool Created)> GetOrCreateAsync(
        string userId,
        string departmentId,
        int year,
        int month,
        Action<UserScheduleRules>? initializeRules = null)
    {
        var monthName = new DateTime(year, month, 1).ToString("MMMM").ToLower();
        var existingRules = await userRuleRepository.GetMonthScheduleRules(
            userId,
            departmentId,
            monthName,
            year);

        if (existingRules is not null)
        {
            var existingSchedule = await scheduleRepository.GetByIdAsync(existingRules.ScheduleId);
            if (existingSchedule is not null)
            {
                return (existingSchedule, existingRules, false);
            }

            var repairedSchedule = new Schedule
            {
                Id = existingRules.ScheduleId,
                MonthName = monthName,
            };
            await scheduleRepository.AddAsync(repairedSchedule);
            return (repairedSchedule, existingRules, false);
        }

        var scheduleId = ObjectId.GenerateNewId().ToString();
        var schedule = new Schedule
        {
            Id = scheduleId,
            MonthName = monthName,
        };
        var rules = new UserScheduleRules
        {
            UserId = userId,
            DepartmentId = departmentId,
            Year = year,
            MonthName = monthName,
            ScheduleId = scheduleId,
        };
        initializeRules?.Invoke(rules);

        await scheduleRepository.AddAsync(schedule);
        try
        {
            await userRuleRepository.AddAsync(rules);
        }
        catch
        {
            await scheduleRepository.RemoveAsync(scheduleId);
            throw;
        }

        return (schedule, rules, true);
    }
}
