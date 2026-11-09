namespace UserManagementService.DataAccess.Repository;

using Microsoft.EntityFrameworkCore;
using UserManagementService.DataAccess.Database;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;

public class GenericRepository<TEntity>(UserManagementDbContext context) : IGenericRepository<TEntity>
    where TEntity : Entity
{
    protected readonly DbSet<TEntity> dbSet = context.Set<TEntity>();

    public async Task AddAsync(TEntity obj) => await dbSet.AddAsync(obj);

    public async Task<IEnumerable<TEntity>> GetAllAsync() => await dbSet.ToListAsync();

    public async Task<TEntity> GetByIdAsync(string id) => (await dbSet.FindAsync(EntityId.Normalize(id)))!;

    public Task<List<TEntity>> GetByIdsAsync(IEnumerable<string> ids)
    {
        var normalized = ids.Select(EntityId.Normalize).ToList();
        return dbSet.Where(entity => normalized.Contains(entity.Id)).ToListAsync();
    }

    public async Task RemoveAsync(string id)
    {
        var entity = await dbSet.FindAsync(EntityId.Normalize(id));
        if (entity is not null) dbSet.Remove(entity);
    }

    public Task UpdateAsync(TEntity obj)
    {
        dbSet.Update(obj);
        return Task.CompletedTask;
    }
}
