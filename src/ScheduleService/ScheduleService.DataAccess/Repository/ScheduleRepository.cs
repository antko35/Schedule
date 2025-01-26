namespace ScheduleService.DataAccess.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;
    using ScheduleService.DataAccess.Database;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;
    using static System.Net.Mime.MediaTypeNames;

    public class ScheduleRepository : GenericRepository<Schedule>, IScheduleRepository
    {
        public ScheduleRepository(DbContext context, DbOptions options)
            : base(context, options.ScheduleCollection)
        {
        }

        public async Task AddWorkDayAsync(string scheduleId, WorkDay newWorkDay)
        {
            var filter = Builders<Schedule>.Filter.Eq(x => x.Id, scheduleId);

            var update = Builders<Schedule>.Update.Push(s => s.WorkDays, newWorkDay);

            var result = await dbSet.UpdateOneAsync(filter, update);
        }

        public async Task<Schedule?> GetWorkDayAsync(string scheduleId, int day)
        {
            var filter = Builders<Schedule>.Filter.And(
                Builders<Schedule>.Filter.Eq(x => x.Id, scheduleId),
                Builders<Schedule>.Filter.ElemMatch(x => x.WorkDays, wd => wd.Day == day));

            var result = await dbSet.Find(filter).FirstOrDefaultAsync();

            // Возврат первого подходящего рабочего дня
            return result;
        }

        public async Task UpdateWorkDayAsync(string scheduleId, WorkDay newWorkDay)
        {
            var filter = Builders<Schedule>.Filter.And(
                Builders<Schedule>.Filter.Eq(x => x.Id, scheduleId));

            var update = Builders<Schedule>.Update.Set(
                s => s.WorkDays[-1].StartTime, newWorkDay.StartTime)
            .Set(
                s => s.WorkDays[-1].EndTime, newWorkDay.EndTime);

            await dbSet.UpdateOneAsync(filter, update);
        }
    }
}
