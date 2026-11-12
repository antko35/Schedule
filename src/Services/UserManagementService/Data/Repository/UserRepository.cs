namespace UserManagementService.DataAccess.Repository;

using Microsoft.EntityFrameworkCore;
using UserManagementService.DataAccess.Database;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;

public class UserRepository(UserManagementDbContext context)
    : GenericRepository<User>(context), IUserRepository
{
    public Task<List<ShortUserInfo>> GetShortUsersInfo() =>
        dbSet.Select(user => new ShortUserInfo
        {
            Id = user.Id, FirstName = user.FirstName!, LastName = user.LastName!, Patronymic = user.Patronymic!,
        }).ToListAsync();

    public Task<List<ShortUserInfo>> GetAvailableUsersForDepartmentAsync(
        IEnumerable<string> existingUserIds, CancellationToken cancellationToken)
    {
        var excluded = existingUserIds.Select(EntityId.Normalize).ToList();
        return dbSet.Where(user => !excluded.Contains(user.Id))
            .OrderBy(user => user.LastName).ThenBy(user => user.FirstName)
            .ThenBy(user => user.Patronymic).ThenBy(user => user.Id)
            .Select(user => new ShortUserInfo
            {
                Id = user.Id, FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty, Patronymic = user.Patronymic ?? string.Empty,
            }).ToListAsync(cancellationToken);
    }
}
