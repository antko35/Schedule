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

        public async Task<WorkDay?> GetWorkDayAsync(string scheduleId, int day)
        {
            var filter = Builders<Schedule>.Filter.And(
               Builders<Schedule>.Filter.Eq(x => x.Id, scheduleId));

            var schedule = await dbSet.Find(filter).FirstOrDefaultAsync();

            var workDay = schedule.WorkDays.Where(x => x.StartTime.Day == day).FirstOrDefault();

            return workDay;
        }
    }
}
