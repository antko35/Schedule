using MongoDB.Driver;
using UserManagementService.Data.Database;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;

namespace UserManagementService.Data.Repository;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(DbContext context)
    {
        _users = context.Database.GetCollection<User>("Users");
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _users.Find(_ => true).ToListAsync();
    }
}
