using MongoDB.Driver;
using MongoDB.Driver.Linq;
using UserManagementService.Application.DTOs;

namespace UserManagementService.DataAccess.Repository;

using UserManagementService.DataAccess.Database;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(DbContext context,
                          BaseDbOptions options) : base(context, options.UsersCollectionName)
    {
    }

    public async Task<List<ShortUserInfo>> GetShortUsersInfo()
    {
        return await dbSet.AsQueryable()
            .Select(u => new ShortUserInfo
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Patronymic = u.Patronymic
            })
            .ToListAsync();
    }

    public async Task<List<ShortUserInfo>> GetAvailableUsersForDepartmentAsync(
        IEnumerable<string> existingUserIds,
        CancellationToken cancellationToken)
    {
        var excludedUserIds = existingUserIds.ToList();

        return await dbSet.AsQueryable()
            .Where(user => !excludedUserIds.Contains(user.Id))
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ThenBy(user => user.Patronymic)
            .ThenBy(user => user.Id)
            .Select(user => new ShortUserInfo
            {
                Id = user.Id,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Patronymic = user.Patronymic ?? string.Empty,
            })
            .ToListAsync(cancellationToken);
    }
}
