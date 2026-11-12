namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule;
using MediatR;
using ScheduleService.DataAccess.Database;
using ScheduleService.Application.Extensions;
using ScheduleService.Application.Extensions.Validation;
using ScheduleService.Application.Services;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;
public sealed class GenerateScheduleCommandHandler(
    IScheduleGenerationRuleRepository ruleRepository, IScheduleRepository scheduleRepository,
    ICalendarRepository calendarRepository, ScheduleDbContext dbContext)
    : IRequestHandler<GenerateScheduleCommand>
{
    public async Task Handle(GenerateScheduleCommand request, CancellationToken cancellationToken)
    {
        var period = new DateOnly(request.Year, request.Month, 1);
        var rule = await ruleRepository.GetAsync(request.UserId, request.DepartmentId, period, cancellationToken);
        rule.EnsureExists("Schedule rules for this user not found");
        var errors = new Dictionary<string, string[]>();
        if (rule!.TargetMinutes <= 0) errors["HoursPerMonth"] = ["Hours per month must be greater than zero."];
        if (rule.MaxShiftMinutes <= 0) errors["MaxHoursPerDay"] = ["Hours per day must be greater than zero."];
        if (rule.MaxShiftMinutes > 0 && (double)rule.TargetMinutes / rule.MaxShiftMinutes is < 15 or > 23)
            errors["WorkDays"] = ["The generated work day count must be between 15 and 23."];
        if (rule.Pattern == ShiftPattern.Manual) errors["Periodicity"] = ["An automatic schedule periodicity must be selected."];
        if (errors.Count > 0) throw new ValidationException(errors);
        var schedule = await scheduleRepository.GetAsync(request.UserId, request.DepartmentId, period, cancellationToken);
        schedule.EnsureExists("Schedule not found");
        var holidays = await calendarRepository.GetMonthHolidaysAsync(period, cancellationToken);
        var transfers = await calendarRepository.GetMonthTransferDaysAsync(period, cancellationToken);
        var shifts = ScheduleGenerator.GenerateWorkShiftsForUser(rule, holidays, transfers, schedule!.Id);
        schedule.ReplaceShifts(shifts, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
