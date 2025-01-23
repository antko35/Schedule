namespace ScheduleService.Domain.Abstractions
{
    using ScheduleService.Domain.Models;

    public interface IGenericRepository<TEntity> where TEntity : Entity
    {
        Task AddAsync(TEntity obj);

        void Dispose();

        Task<IEnumerable<TEntity>> GetAllAsync();

        Task<TEntity> GetByIdAsync(string id);

        Task RemoveAsync(string id);

        Task UpdateAsync(TEntity updObj);
    }
}