namespace UserManagementService.DataAccess.Repository;

using MongoDB.Driver;
using UserManagementService.DataAccess.Database;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(DbContext context,
                          BaseDbOptions options) : base(context, options.UsersCollectionName)
    {
    }
}
