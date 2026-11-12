namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule;

using MediatR;
using ScheduleService.DataAccess.Database;
using ScheduleService.Application.Contracts;
using ScheduleService.Application.Extensions;
using ScheduleService.Application.Mapping;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;

public sealed class CreateWorkDayManuallyCommandHandler
    : IRequestHandler<CreateWorkDayManuallyCommand, WorkDayDto>
{
    private readonly ScheduleService.Application.Services.ScheduleMonthContextService monthContextService;
    private readonly ScheduleDbContext dbContext;

    public CreateWorkDayManuallyCommandHandler(
        ScheduleService.Application.Services.ScheduleMonthContextService monthContextService,
        ScheduleDbContext dbContext)
    {
        this.monthContextService = monthContextService;
        this.dbContext = dbContext;
    }

    public async Task<WorkDayDto> Handle(
        CreateWorkDayManuallyCommand request,
        CancellationToken cancellationToken)
    {
        var startsAtUtc = AsUtc(request.StartTime);
        var endsAtUtc = AsUtc(request.EndTime);
        var period = new DateOnly(startsAtUtc.Year, startsAtUtc.Month, 1);

        var context = await monthContextService.GetOrCreateAsync(
            request.UserId,
            request.DepartmentId,
            period.Year,
            period.Month,
            cancellationToken);

        var schedule = context.Schedule;

        var shift = new WorkShift(
            Guid.NewGuid(),
            schedule!.Id,
            DateOnly.FromDateTime(startsAtUtc),
            startsAtUtc,
            endsAtUtc);

        schedule.SetShift(shift, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return shift.ToDto();
    }

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
    };
}
