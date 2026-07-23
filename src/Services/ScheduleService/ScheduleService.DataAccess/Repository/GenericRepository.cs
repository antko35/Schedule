namespace ScheduleService.DataAccess.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MongoDB.Driver;
    using ScheduleService.DataAccess.Database;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class GenericRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : Entity
    {
        private readonly DbContext context;
        protected IMongoCollection<TEntity> dbSet;

        public GenericRepository(DbContext context, string collectionName)
        {
            this.context = context;
            dbSet = context.Database.GetCollection<TEntity>(collectionName);
        }

        public async Task AddAsync(TEntity obj)
        {
            await dbSet.InsertOneAsync(obj);
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> objs)
        {
            await dbSet.InsertManyAsync(objs);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await dbSet.Find(_ => true).ToListAsync();
        }

        public async Task<TEntity> GetByIdAsync(string id)
        {
            return await dbSet.Find(obj => obj.Id == id).FirstOrDefaultAsync();
        }

        public async Task RemoveAsync(string id)
        {
            await dbSet.DeleteOneAsync(obj => obj.Id == id);
        }

        public async Task UpdateAsync(TEntity updObj)
        {
            await dbSet.ReplaceOneAsync(obj => obj.Id == updObj.Id, updObj);
        }
    }
}
