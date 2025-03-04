namespace UserManagementService.DataAccess.Repository;

using MongoDB.Driver;
using UserManagementService.DataAccess.Database;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;
using ZstdSharp.Unsafe;

public class UserJobsRepository : GenericRepository<UserJob>, IUserJobsRepository
{
    public UserJobsRepository(DbContext dbContext, BaseDbOptions options)
        : base(dbContext, options.UserJobsCollectionName)
    {
    }

    public async Task<UserJob> GetUserJobAsync(string userId, string departmentId)
    {
        var filter = Builders<UserJob>.Filter.And(
       Builders<UserJob>.Filter.Eq(x => x.UserId, userId),
       Builders<UserJob>.Filter.Eq(x => x.DepartmentId, departmentId));

        return await dbSet.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<UserJob>> GetUserJobsByDepartmentIdAsync(string departmentId)
    {
        var filter = Builders<UserJob>.Filter.Eq(x => x.DepartmentId, departmentId);

        var userJobs = await dbSet.Find(filter).ToListAsync();

        return userJobs;
    }

    public async Task<IEnumerable<UserJob>> GetUserJobsByUserIdAsync(string userId)
    {
        var filter = Builders<UserJob>.Filter.Eq(x => x.UserId, userId);

        var userJobs = await dbSet.Find(filter).ToListAsync();

        return userJobs;
    }

    public async Task<List<string>> GetHeadEmails(IEnumerable<string> departmentIds)
    {
        var filter = Builders<UserJob>.Filter.And(
            Builders<UserJob>.Filter.In(u => u.DepartmentId, departmentIds),
            Builders<UserJob>.Filter.Eq(u => u.Role, "departmentHead"));

        var emails = await dbSet
            .Find(filter)
            .ToListAsync();

        return emails.Select(u => u.Email).ToList();
    }

    public async Task DeleteByUserId(string userId)
    {
        var filter = Builders<UserJob>.Filter.Eq(x => x.UserId, userId);

        await dbSet.DeleteManyAsync(filter);
    }
}
