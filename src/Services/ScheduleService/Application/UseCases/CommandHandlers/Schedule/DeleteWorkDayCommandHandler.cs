namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule;

using MediatR;
using ScheduleService.DataAccess.Database;
using ScheduleService.Application.Contracts;
using ScheduleService.Application.Extensions;
using ScheduleService.Application.Mapping;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.Domain.Abstractions;

public sealed class DeleteWorkDayCommandHandler
    : IRequestHandler<DeleteWorkDayCommand, ScheduleDto>
{
    private readonly IScheduleRepository scheduleRepository;
    private readonly ScheduleDbContext dbContext;

    public DeleteWorkDayCommandHandler(
        IScheduleRepository scheduleRepository,
        ScheduleDbContext dbContext)
    {
        this.scheduleRepository = scheduleRepository;
        this.dbContext = dbContext;
    }

    public async Task<ScheduleDto> Handle(
        DeleteWorkDayCommand request,
        CancellationToken cancellationToken)
    {
        var workDate = DateOnly.FromDateTime(request.WorkDay);
        var period = new DateOnly(workDate.Year, workDate.Month, 1);
        var schedule = await scheduleRepository.GetAsync(
            request.UserId,
            request.DepartmentId,
            period,
            cancellationToken);
        schedule.EnsureExists("Schedule for this user not found");

        if (!schedule!.RemoveShift(workDate, DateTime.UtcNow))
        {
            throw new KeyNotFoundException("No work during day");
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return schedule.ToDto();
    }
}
