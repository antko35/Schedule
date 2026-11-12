namespace UserManagementService.DataAccess.Repository;

using Microsoft.EntityFrameworkCore;
using UserManagementService.DataAccess.Database;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;

public class UserJobsRepository(UserManagementDbContext context)
    : GenericRepository<UserJob>(context), IUserJobsRepository
{
    public async Task<UserJob> GetUserJobAsync(string userId, string departmentId)
    {
        userId = EntityId.Normalize(userId);
        departmentId = EntityId.Normalize(departmentId);
        return (await dbSet.FirstOrDefaultAsync(job => job.UserId == userId && job.DepartmentId == departmentId))!;
    }

    public async Task<IEnumerable<UserJob>> GetUserJobsByDepartmentIdAsync(string departmentId)
    {
        departmentId = EntityId.Normalize(departmentId);
        return await dbSet.Where(job => job.DepartmentId == departmentId).ToListAsync();
    }

    public async Task<IEnumerable<UserJob>> GetUserJobsByUserIdAsync(string userId)
    {
        userId = EntityId.Normalize(userId);
        return await dbSet.Where(job => job.UserId == userId).ToListAsync();
    }

    public Task<List<string>> GetHeadEmails(IEnumerable<string> departmentIds)
    {
        var normalized = departmentIds.Select(EntityId.Normalize).ToList();
        return dbSet.Where(job => normalized.Contains(job.DepartmentId!) && job.Role == UserJobRole.DepartmentHead)
            .Select(job => job.Email!).ToListAsync();
    }

    public async Task DeleteByUserId(string userId)
    {
        userId = EntityId.Normalize(userId);
        dbSet.RemoveRange(await dbSet.Where(job => job.UserId == userId).ToListAsync());
    }
}
