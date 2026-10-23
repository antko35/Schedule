using MediatR;
using MongoDB.Bson;
using ScheduleService.Application.Services;
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
    private readonly ScheduleRulesGenerator scheduleRulesGenerator;

    public AddingScheduleRulesForNextMonthCommandHandler(
        IUserRuleRepository scheduleRuleRepository,
        IScheduleRepository scheduleRepository, ScheduleRulesGenerator scheduleRulesGenerator)
    {
        this.scheduleRuleRepository = scheduleRuleRepository;
        this.scheduleRepository = scheduleRepository;
        this.scheduleRulesGenerator = scheduleRulesGenerator;
    }

    public async Task Handle(AddingScheduleRulesForNextMonthCommand request, CancellationToken cancellationToken)
    {
        var currentDate = DateTime.Now;
        var nextMonthDate = currentDate.AddMonths(1);

        var currentMonth = currentDate.ToString("MMMM").ToLower();
        var nextMonth = nextMonthDate.ToString("MMMM").ToLower();

        var existingScheduleRules = await scheduleRuleRepository.GetAllRulesByMonth(currentMonth);

        if (existingScheduleRules == null)
        {
            throw new InvalidOperationException("No ScheduleRules");
        }

        foreach (var scheduleRule in existingScheduleRules)
        {
            await scheduleRulesGenerator.CreateAndAddRulesAndSchedules(scheduleRule.UserId, scheduleRule.DepartmentId, nextMonthDate, nextMonth);
        }
    }
}