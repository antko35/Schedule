namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule;

using MediatR;
using ScheduleService.DataAccess.Database;
using ScheduleService.Application.Services;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;

public sealed class GenerateDepartmentScheduleCommandHandler
    : IRequestHandler<GenerateDepartmentScheduleCommand>
{
    private readonly IScheduleGenerationRuleRepository ruleRepository;
    private readonly IScheduleRepository scheduleRepository;
    private readonly ICalendarRepository calendarRepository;
    private readonly ScheduleDbContext dbContext;

    public GenerateDepartmentScheduleCommandHandler(
        IScheduleGenerationRuleRepository ruleRepository,
        IScheduleRepository scheduleRepository,
        ICalendarRepository calendarRepository,
        ScheduleDbContext dbContext)
    {
        this.ruleRepository = ruleRepository;
        this.scheduleRepository = scheduleRepository;
        this.calendarRepository = calendarRepository;
        this.dbContext = dbContext;
    }

    public async Task Handle(
        GenerateDepartmentScheduleCommand request,
        CancellationToken cancellationToken)
    {
        var period = new DateOnly(request.Year, request.Month, 1);
        var rules = await ruleRepository.GetByDepartmentAsync(
            request.DepartmentId,
            period,
            cancellationToken);
        if (rules.Count == 0)
        {
            throw new InvalidOperationException("Schedule rules for this department not found");
        }

        EnsureRulesCanBeGenerated(rules);

        // All repositories share one scoped EF Core DbContext. Its async queries must not overlap.
        var holidays = await calendarRepository.GetMonthHolidaysAsync(period, cancellationToken);
        var transferDays = await calendarRepository.GetMonthTransferDaysAsync(period, cancellationToken);

        var updatedAtUtc = DateTime.UtcNow;
        foreach (var rule in rules)
        {
            var schedule = await scheduleRepository.GetAsync(
                rule.UserId,
                rule.DepartmentId,
                rule.Period,
                cancellationToken)
                ?? throw new KeyNotFoundException($"Schedule for rule {rule.Id} not found");
            var shifts = ScheduleGenerator.GenerateWorkShiftsForUser(
                rule,
                holidays,
                transferDays,
                schedule.Id);
            schedule.ReplaceShifts(shifts, updatedAtUtc);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureRulesCanBeGenerated(
        IReadOnlyList<ScheduleGenerationRule> rules)
    {
        var invalidRule = rules.FirstOrDefault(rule =>
            rule.TargetMinutes <= 0
            || rule.MaxShiftMinutes <= 0
            || rule.Pattern == ShiftPattern.Manual);

        if (invalidRule is not null)
        {
            throw new InvalidOperationException(
                $"Schedule rule {invalidRule.Id} must define positive hours and a shift pattern before generation.");
        }
    }
}
