namespace UserManagementService.Domain.Abstractions.IRepository;

using UserManagementService.Domain.Models;

public interface IUserRepository : IGenericRepository<User>
{
    Task<List<ShortUserInfo>> GetShortUsersInfo();
}