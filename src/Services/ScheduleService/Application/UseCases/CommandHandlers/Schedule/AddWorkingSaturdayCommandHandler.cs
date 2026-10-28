namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule;

using MediatR;
using ScheduleService.DataAccess.Database;
using ScheduleService.Application.Contracts;
using ScheduleService.Application.Extensions;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;

public sealed class AddWorkingSaturdayCommandHandler : IRequestHandler<AddWorkingSaturdayCommand>
{
    private readonly IScheduleRepository scheduleRepository;
    private readonly ScheduleDbContext dbContext;

    public AddWorkingSaturdayCommandHandler(
        IScheduleRepository scheduleRepository,
        ScheduleDbContext dbContext)
    {
        this.scheduleRepository = scheduleRepository;
        this.dbContext = dbContext;
    }

    public async Task Handle(
        AddWorkingSaturdayCommand request,
        CancellationToken cancellationToken)
    {
        var schedule = await scheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken);
        schedule.EnsureExists("Schedule not found");

        var saturdayStartUtc = AsUtc(request.Saturday.StartTime);
        var saturdayEndUtc = AsUtc(request.Saturday.EndTime);
        var saturdayDate = DateOnly.FromDateTime(saturdayStartUtc);
        if (schedule!.WorkShifts.Any(shift => shift.WorkDate == saturdayDate))
        {
            throw new InvalidOperationException("Saturday already exist");
        }

        if (request.WorkingDays.Count != 5)
        {
            throw new InvalidOperationException("Should be 5 updated days");
        }

        var replacementShifts = new List<WorkShift>(request.WorkingDays.Count);
        var oldTotalTime = TimeSpan.Zero;
        var newTotalTime = TimeSpan.Zero;
        foreach (var workDay in request.WorkingDays)
        {
            var startsAtUtc = AsUtc(workDay.StartTime);
            var endsAtUtc = AsUtc(workDay.EndTime);
            var workDate = DateOnly.FromDateTime(startsAtUtc);
            var existingShift = schedule.WorkShifts.SingleOrDefault(shift => shift.WorkDate == workDate)
                ?? throw new KeyNotFoundException($"No work found for this day ({workDate:yyyy-MM-dd})");
            var duration = endsAtUtc - startsAtUtc;
            if (duration < TimeSpan.FromHours(4))
            {
                throw new InvalidOperationException("Minimum work day length is 4 hours");
            }

            oldTotalTime += existingShift.EndsAtUtc - existingShift.StartsAtUtc;
            newTotalTime += duration;
            replacementShifts.Add(new WorkShift(
                existingShift.Id,
                schedule.Id,
                workDate,
                startsAtUtc,
                endsAtUtc));
        }

        if (oldTotalTime - newTotalTime != saturdayEndUtc - saturdayStartUtc)
        {
            throw new InvalidOperationException($"Invalid time difference {oldTotalTime - newTotalTime}");
        }

        var updatedAtUtc = DateTime.UtcNow;
        foreach (var shift in replacementShifts)
        {
            schedule.SetShift(shift, updatedAtUtc);
        }

        schedule.SetShift(
            new WorkShift(
                Guid.NewGuid(),
                schedule.Id,
                saturdayDate,
                saturdayStartUtc,
                saturdayEndUtc),
            updatedAtUtc);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
    };
}
