namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule;

using MediatR;
using ScheduleService.DataAccess.Database;
using ScheduleService.Application.Extensions;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.Domain.Abstractions;

public sealed class DeleteUserMonthScheduleCommandHandler
    : IRequestHandler<DeleteUserMonthScheduleCommand, string>
{
    private readonly IScheduleRepository scheduleRepository;
    private readonly ScheduleDbContext dbContext;

    public DeleteUserMonthScheduleCommandHandler(
        IScheduleRepository scheduleRepository,
        ScheduleDbContext dbContext)
    {
        this.scheduleRepository = scheduleRepository;
        this.dbContext = dbContext;
    }

    public async Task<string> Handle(
        DeleteUserMonthScheduleCommand request,
        CancellationToken cancellationToken)
    {
        var period = new DateOnly(request.Year, request.Month, 1);
        var schedule = await scheduleRepository.GetAsync(
            request.UserId,
            request.DepartmentId,
            period,
            cancellationToken);
        schedule.EnsureExists("Invalid input");

        if (schedule!.WorkShifts.Count == 0)
        {
            return "Nothing to delete";
        }

        schedule.ReplaceShifts(Array.Empty<Domain.Models.WorkShift>(), DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return $" Schedule {schedule.Id} was cleaned.";
    }
}
