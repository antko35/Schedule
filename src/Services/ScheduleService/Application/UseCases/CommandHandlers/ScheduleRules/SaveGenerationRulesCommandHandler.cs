namespace ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules;

using MediatR;
using ScheduleService.DataAccess.Database;
using ScheduleService.Application.Contracts;
using ScheduleService.Application.Mapping;
using ScheduleService.Application.Services;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;

public sealed class SaveGenerationRulesCommandHandler(
    ScheduleMonthContextService monthContextService, ScheduleDbContext dbContext)
    : IRequestHandler<SaveGenerationRulesCommand, ScheduleRulesDto>
{
    public async Task<ScheduleRulesDto> Handle(SaveGenerationRulesCommand request, CancellationToken cancellationToken)
    {
        var context = await monthContextService.GetOrCreateAsync(
            request.UserId, request.DepartmentId, request.Year, request.Month, cancellationToken);
        var pattern = request switch
        {
            { OnlyFirstShift: true } => ShiftPattern.FirstShiftOnly,
            { OnlySecondShift: true } => ShiftPattern.SecondShiftOnly,
            { EvenDOW: true } => ShiftPattern.AlternateByWeekdayA,
            { UnEvenDOW: true } => ShiftPattern.AlternateByWeekdayB,
            { EvenDOM: true } => ShiftPattern.AlternateByMonthDayA,
            { UnEvenDOM: true } => ShiftPattern.AlternateByMonthDayB,
            _ => ShiftPattern.Manual,
        };
        context.Rules.Reconfigure(
            checked((int)Math.Round(request.HoursPerMonth * 60, MidpointRounding.AwayFromZero)),
            checked((int)Math.Round(request.MaxHoursPerDay * 60, MidpointRounding.AwayFromZero)),
            request.StartWorkDayTime, context.Rules.SecondShiftStart, pattern);
        await dbContext.SaveChangesAsync(cancellationToken);
        return context.Rules.ToDto(context.Schedule.Id);
    }
}
