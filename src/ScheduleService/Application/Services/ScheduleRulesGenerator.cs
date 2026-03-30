namespace ScheduleService.Application.Services;
using MongoDB.Bson;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;

public class ScheduleRulesGenerator
{
    private readonly IScheduleRepository scheduleRepository;
    private readonly IUserRuleRepository scheduleRuleRepository;

    public ScheduleRulesGenerator(IUserRuleRepository scheduleRuleRepository, IScheduleRepository scheduleRepository)
    {
        this.scheduleRuleRepository = scheduleRuleRepository;
        this.scheduleRepository = scheduleRepository;
    }

    public async Task CreateAndAddRulesAndSchedules(
        string userId, string departmentId, DateTime nextMonthDate, string nextMonth)
    {
        var scheduleId = ObjectId.GenerateNewId().ToString();

        var schedule = new Domain.Models.Schedule
        {
            Id = scheduleId,
            MonthName = nextMonth,
        };

        var scheduleRules = new UserScheduleRules
        {
            UserId = userId,
            DepartmentId = departmentId,
            Year = nextMonthDate.Year,
            MonthName = nextMonth,
            ScheduleId = scheduleId,
        };

        await scheduleRuleRepository.AddAsync(scheduleRules);
        await scheduleRepository.AddAsync(schedule);
    }
}