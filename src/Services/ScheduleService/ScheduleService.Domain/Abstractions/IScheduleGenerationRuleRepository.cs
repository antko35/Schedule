namespace ScheduleService.Domain.Abstractions;

using ScheduleService.Domain.Models;

public interface IScheduleGenerationRuleRepository
{
    Task<ScheduleGenerationRule?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task<ScheduleGenerationRule?> GetAsync(
        string userId,
        string departmentId,
        DateOnly period,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ScheduleGenerationRule>> GetByDepartmentAsync(
        string departmentId,
        DateOnly period,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ScheduleGenerationRule>> GetAllByMonthAsync(
        DateOnly period,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ScheduleGenerationRule>> GetAllAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<ScheduleGenerationRule>> GetAllByUserDepartmentAsync(
        string userId,
        string departmentId,
        CancellationToken cancellationToken);

    Task AddAsync(ScheduleGenerationRule rule, CancellationToken cancellationToken);

    void Remove(ScheduleGenerationRule rule);

    void RemoveRange(IEnumerable<ScheduleGenerationRule> rules);
}
