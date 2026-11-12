namespace ScheduleService.DataAccess.Repository;

using Microsoft.EntityFrameworkCore;
using ScheduleService.DataAccess.Database;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;

public sealed class ScheduleRepository : IScheduleRepository
{
    private readonly ScheduleDbContext context;

    public ScheduleRepository(ScheduleDbContext context)
    {
        this.context = context;
    }

    public Task<MonthlySchedule?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return context.MonthlySchedules
            .Include(schedule => schedule.WorkShifts)
            .SingleOrDefaultAsync(schedule => schedule.Id == id, cancellationToken);
    }

    public Task<MonthlySchedule?> GetAsync(
        string userId,
        string departmentId,
        DateOnly period,
        CancellationToken cancellationToken)
    {
        return context.MonthlySchedules
            .Include(schedule => schedule.WorkShifts)
            .SingleOrDefaultAsync(
                schedule => schedule.UserId == userId
                    && schedule.DepartmentId == departmentId
                    && schedule.Period == period,
                cancellationToken);
    }

    public async Task<IReadOnlyList<MonthlySchedule>> GetDepartmentAsync(
        string departmentId,
        DateOnly period,
        CancellationToken cancellationToken)
    {
        return await context.MonthlySchedules
            .AsNoTracking()
            .Include(schedule => schedule.WorkShifts)
            .Where(schedule => schedule.DepartmentId == departmentId && schedule.Period == period)
            .OrderBy(schedule => schedule.UserId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MonthlySchedule>> GetEmptyAsync(
        DateOnly period,
        CancellationToken cancellationToken)
    {
        return await context.MonthlySchedules
            .Where(schedule => schedule.Period == period && !schedule.WorkShifts.Any())
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(MonthlySchedule schedule, CancellationToken cancellationToken)
    {
        return context.MonthlySchedules.AddAsync(schedule, cancellationToken).AsTask();
    }

    public void Remove(MonthlySchedule schedule)
    {
        context.MonthlySchedules.Remove(schedule);
    }
}
