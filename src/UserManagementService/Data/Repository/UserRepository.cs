namespace UserManagementService.DataAccess.Repository;

using MongoDB.Driver;
using UserManagementService.DataAccess.Database;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(DbContext context,
                          IDbOptions options) : base(context, options.UsersCollectionName)
    {
    }

    //private readonly IMongoCollection<User> users;

    //public UserRepository(DbContext context, IDbOptions settings)
    //{
    //    users = context.Database.GetCollection<User>(settings.UsersCollectionName);
    //}

    //public async Task<IEnumerable<User>> GetAllAsync()
    //{
    //    return await users.Find(_ => true).ToListAsync();
    //}

    //public async Task<User> GetByIdAsync(string id)
    //{
    //    return await users.Find(user => user.Id == id).FirstOrDefaultAsync();
    //}

    //public async Task<User> CreateAsync(User user)
    //{
    //    await users.InsertOneAsync(user);
    //    return user;
    //}

    //public async Task<User> UpdateAsync(string id, User updatedUser)
    //{
    //    await users.ReplaceOneAsync(user => user.Id == id, updatedUser);
    //    return updatedUser;
    //}

    //public async Task DeleteAsync(string id)
    //{
    //    await users.DeleteOneAsync(user => user.Id == id);
    //}
}
