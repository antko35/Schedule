namespace ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules;

using MediatR;
using ScheduleService.DataAccess.Database;
using ScheduleService.Application.Contracts;
using ScheduleService.Application.Mapping;
using ScheduleService.Application.Extensions;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;

public sealed class SetGenerationRulesCommandHandler : IRequestHandler<SetGenerationRulesCommand, ScheduleRulesDto>
{
    private readonly IScheduleGenerationRuleRepository ruleRepository;
    private readonly ScheduleDbContext dbContext;
    private readonly IScheduleRepository scheduleRepository;

    public SetGenerationRulesCommandHandler(
        IScheduleGenerationRuleRepository ruleRepository,
        ScheduleDbContext dbContext,
        IScheduleRepository scheduleRepository)
    {
        this.ruleRepository = ruleRepository;
        this.dbContext = dbContext;
        this.scheduleRepository = scheduleRepository;
    }

    public async Task<ScheduleRulesDto> Handle(
        SetGenerationRulesCommand request,
        CancellationToken cancellationToken)
    {
        var rule = await ruleRepository.GetByIdAsync(request.ScheduleRulesId, cancellationToken);
        rule.EnsureExists("Schedule rules not found");

        rule!.Reconfigure(
            request.HoursPerMonth.HasValue ? ToMinutes(request.HoursPerMonth.Value) : rule.TargetMinutes,
            request.MaxHoursPerDay.HasValue ? ToMinutes(request.MaxHoursPerDay.Value) : rule.MaxShiftMinutes,
            request.StartWorkDayTime ?? rule.FirstShiftStart,
            rule.SecondShiftStart,
            GetPattern(request, rule.Pattern));
        await dbContext.SaveChangesAsync(cancellationToken);
        var schedule = await scheduleRepository.GetAsync(rule.UserId, rule.DepartmentId, rule.Period, cancellationToken);
        return rule.ToDto(schedule?.Id ?? string.Empty);
    }

    private static int ToMinutes(float hours) =>
        checked((int)Math.Round(hours * 60, MidpointRounding.AwayFromZero));

    private static ShiftPattern GetPattern(SetGenerationRulesCommand request, ShiftPattern current)
    {
        var hasPatternUpdate = new[]
        {
            request.OnlyFirstShift, request.OnlySecondShift, request.EvenDOW,
            request.UnEvenDOW, request.EvenDOM, request.UnEvenDOM,
        }.Any(flag => flag.HasValue);

        return request switch
        {
            { OnlyFirstShift: true } => ShiftPattern.FirstShiftOnly,
            { OnlySecondShift: true } => ShiftPattern.SecondShiftOnly,
            { EvenDOW: true } => ShiftPattern.AlternateByWeekdayA,
            { UnEvenDOW: true } => ShiftPattern.AlternateByWeekdayB,
            { EvenDOM: true } => ShiftPattern.AlternateByMonthDayA,
            { UnEvenDOM: true } => ShiftPattern.AlternateByMonthDayB,
            _ => hasPatternUpdate ? ShiftPattern.Manual : current,
        };
    }
}
