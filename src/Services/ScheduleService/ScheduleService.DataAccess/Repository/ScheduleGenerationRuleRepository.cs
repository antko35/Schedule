namespace ScheduleService.DataAccess.Repository;

using Microsoft.EntityFrameworkCore;
using ScheduleService.DataAccess.Database;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;

public sealed class ScheduleGenerationRuleRepository : IScheduleGenerationRuleRepository
{
    private readonly ScheduleDbContext context;

    public ScheduleGenerationRuleRepository(ScheduleDbContext context)
    {
        this.context = context;
    }

    public Task<ScheduleGenerationRule?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return context.ScheduleGenerationRules
            .SingleOrDefaultAsync(rule => rule.Id == id, cancellationToken);
    }

    public Task<ScheduleGenerationRule?> GetAsync(
        string userId,
        string departmentId,
        DateOnly period,
        CancellationToken cancellationToken)
    {
        return context.ScheduleGenerationRules
            .SingleOrDefaultAsync(
                rule => rule.UserId == userId
                    && rule.DepartmentId == departmentId
                    && rule.Period == period,
                cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduleGenerationRule>> GetByDepartmentAsync(
        string departmentId,
        DateOnly period,
        CancellationToken cancellationToken)
    {
        return await context.ScheduleGenerationRules
            .AsNoTracking()
            .Where(rule => rule.DepartmentId == departmentId && rule.Period == period)
            .OrderBy(rule => rule.UserId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduleGenerationRule>> GetAllByMonthAsync(
        DateOnly period,
        CancellationToken cancellationToken)
    {
        return await context.ScheduleGenerationRules
            .AsNoTracking()
            .Where(rule => rule.Period == period)
            .OrderBy(rule => rule.DepartmentId)
            .ThenBy(rule => rule.UserId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduleGenerationRule>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await context.ScheduleGenerationRules
            .AsNoTracking()
            .OrderBy(rule => rule.Period)
            .ThenBy(rule => rule.DepartmentId)
            .ThenBy(rule => rule.UserId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduleGenerationRule>> GetAllByUserDepartmentAsync(
        string userId,
        string departmentId,
        CancellationToken cancellationToken)
    {
        return await context.ScheduleGenerationRules
            .Where(rule => rule.UserId == userId && rule.DepartmentId == departmentId)
            .OrderBy(rule => rule.Period)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(ScheduleGenerationRule rule, CancellationToken cancellationToken)
    {
        return context.ScheduleGenerationRules.AddAsync(rule, cancellationToken).AsTask();
    }

    public void Remove(ScheduleGenerationRule rule)
    {
        context.ScheduleGenerationRules.Remove(rule);
    }

    public void RemoveRange(IEnumerable<ScheduleGenerationRule> rules)
    {
        context.ScheduleGenerationRules.RemoveRange(rules);
    }
}
