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
    
    
}
