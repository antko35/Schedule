namespace UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;
public interface IGenericRepository<TEntity> where TEntity : Entity
{
    Task AddAsync(TEntity obj);

    Task<TEntity> GetByIdAsync(string id);

    Task<IEnumerable<TEntity>> GetAllAsync();

    Task UpdateAsync(TEntity obj);

    Task RemoveAsync(string id);
}
