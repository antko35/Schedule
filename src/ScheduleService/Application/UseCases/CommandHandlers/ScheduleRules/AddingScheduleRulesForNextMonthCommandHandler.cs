using System.Runtime.InteropServices.JavaScript;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;

namespace ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules;

public class AddingScheduleRulesForNextMonthCommandHandler 
    : IRequestHandler<AddingScheduleRulesForNextMonthCommand>
{
    private readonly IScheduleRepository scheduleRepository;
    private readonly IUserRuleRepository scheduleRuleRepository;

    public AddingScheduleRulesForNextMonthCommandHandler(
        IUserRuleRepository scheduleRuleRepository,
        IScheduleRepository scheduleRepository)
    {
        this.scheduleRuleRepository = scheduleRuleRepository;
        this.scheduleRepository = scheduleRepository;
    }

    public async Task Handle(AddingScheduleRulesForNextMonthCommand request, CancellationToken cancellationToken)
    {
        var currentDate = DateTime.Now;
        var nextMonthDate = currentDate.AddMonths(1);

        var currentMonth = currentDate.ToString("MMMM").ToLower();
        var nextMonth = nextMonthDate.ToString("MMMM").ToLower();

        var allScheduleRules = await scheduleRuleRepository.GetAllRulesByMonth(currentMonth);

        if (allScheduleRules == null)
        {
            Console.Out.WriteLine($"allScheduleRules is empty");
            throw new InvalidOperationException("allScheduleRules is empty");
        }

        await CreateAndAddRulesAndSchedules(allScheduleRules, nextMonthDate, nextMonth);
    }

    private async Task CreateAndAddRulesAndSchedules(
        List<UserScheduleRules> allScheduleRules, DateTime nextMonthDate, string nextMonth)
    {
        var schedulesToAdd = new List<Domain.Models.Schedule>();
        var rulesToAdd = new List<UserScheduleRules>();

        foreach (var scheduleRule in allScheduleRules)
        {
            var scheduleId = ObjectId.GenerateNewId().ToString();

            var schedule = new Domain.Models.Schedule
            {
                Id = scheduleId,
                MonthName = nextMonth,
            };

            var scheduleRules = new UserScheduleRules
            {
                UserId = scheduleRule.UserId,
                DepartmentId = scheduleRule.DepartmentId,
                Year = nextMonthDate.Year,
                MonthName = nextMonth,
                ScheduleId = scheduleId,
            };

            schedulesToAdd.Add(schedule);
            rulesToAdd.Add(scheduleRules);
        }

        await scheduleRuleRepository.AddRangeAsync(rulesToAdd);
        await scheduleRepository.AddRangeAsync(schedulesToAdd);
    }
}