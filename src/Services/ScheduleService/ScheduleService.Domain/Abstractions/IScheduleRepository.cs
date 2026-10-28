namespace ScheduleService.Domain.Abstractions;

using ScheduleService.Domain.Models;

public interface IScheduleRepository
{
    Task<MonthlySchedule?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task<MonthlySchedule?> GetAsync(
        string userId,
        string departmentId,
        DateOnly period,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<MonthlySchedule>> GetDepartmentAsync(
        string departmentId,
        DateOnly period,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<MonthlySchedule>> GetEmptyAsync(
        DateOnly period,
        CancellationToken cancellationToken);

    Task AddAsync(MonthlySchedule schedule, CancellationToken cancellationToken);

    void Remove(MonthlySchedule schedule);
}
