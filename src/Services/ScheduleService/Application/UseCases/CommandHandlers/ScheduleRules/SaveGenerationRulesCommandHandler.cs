using MediatR;
using ScheduleService.Application.Services;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Models;

namespace ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules;

public sealed class SaveGenerationRulesCommandHandler
    : IRequestHandler<SaveGenerationRulesCommand, UserScheduleRules>
{
    private readonly ScheduleMonthContextService monthContextService;
    private readonly IUserRuleRepository userRuleRepository;

    public SaveGenerationRulesCommandHandler(
        ScheduleMonthContextService monthContextService,
        IUserRuleRepository userRuleRepository)
    {
        this.monthContextService = monthContextService;
        this.userRuleRepository = userRuleRepository;
    }

    public async Task<UserScheduleRules> Handle(
        SaveGenerationRulesCommand request,
        CancellationToken cancellationToken)
    {
        var context = await monthContextService.GetOrCreateAsync(
            request.UserId,
            request.DepartmentId,
            request.Year,
            request.Month,
            rules => ApplyValues(rules, request));
        var rules = context.Rules;

        if (!context.Created)
        {
            ApplyValues(rules, request);
            await userRuleRepository.UpdateAsync(rules);
        }

        return rules;
    }

    private static void ApplyValues(
        UserScheduleRules rules,
        SaveGenerationRulesCommand request)
    {
        rules.HoursPerMonth = request.HoursPerMonth;
        rules.MaxHoursPerDay = request.MaxHoursPerDay;
        rules.StartWorkDayTime = request.StartWorkDayTime;
        rules.OnlyFirstShift = request.OnlyFirstShift;
        rules.OnlySecondShift = request.OnlySecondShift;
        rules.EvenDOW = request.EvenDOW;
        rules.UnEvenDOW = request.UnEvenDOW;
        rules.EvenDOM = request.EvenDOM;
        rules.UnEvenDOM = request.UnEvenDOM;
    }
}
