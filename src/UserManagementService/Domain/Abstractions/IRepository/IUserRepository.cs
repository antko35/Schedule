namespace UserManagementService.Domain.Abstractions.IRepository;

using UserManagementService.Domain.Models;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
}