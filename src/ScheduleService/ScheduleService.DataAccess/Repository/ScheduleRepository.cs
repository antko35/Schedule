namespace ScheduleService.DataAccess.Repository
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
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

        public async Task DeleteWorkDayAsync(string scheduleId, int day)
        {
            var filer = Builders<Schedule>.Filter.Eq(x => x.Id, scheduleId);

            var update = Builders<Schedule>.Update
                .PullFilter(x => x.WorkDays, x => x.Day == day);

            var result = await dbSet.UpdateOneAsync(filer, update);
        }

        public async Task<Schedule?> GetWorkDayAsync(string scheduleId, int day)
        {
            var filter = Builders<Schedule>.Filter.And(
                Builders<Schedule>.Filter.Eq(x => x.Id, scheduleId),
                Builders<Schedule>.Filter.ElemMatch(x => x.WorkDays, wd => wd.Day == day));

            var result = await dbSet.Find(filter).FirstOrDefaultAsync();

            return result;
        }

        public async Task<Schedule> GetMonthSchedule(string scheduleId)
        {
            var filter = Builders<Schedule>.Filter.Eq(x => x.Id, scheduleId);

            var result = await dbSet.FindAsync(filter);

            return await result.FirstOrDefaultAsync();
        }

        public async Task<List<Schedule>?> GetEmptySchedules(int year, string month)
        {
            var filter = Builders<Schedule>.Filter.And(
                Builders<Schedule>.Filter
                    .Where(s => s.WorkDays.Count == 0),
                Builders<Schedule>.Filter.Eq(s => s.MonthName, month));

            var result = await dbSet.Find(filter).ToListAsync();

            return result;
        }

        public async Task UpdateWorkDayAsync(string scheduleId, WorkDay newWorkDay)
        {
            var filter = Builders<Schedule>.Filter.And(
                Builders<Schedule>.Filter.Eq(x => x.Id, scheduleId),
                Builders<Schedule>.Filter.ElemMatch(x => x.WorkDays, wd => wd.Day == newWorkDay.Day));

            var update = Builders<Schedule>.Update
                .Set("WorkDays.$.StartTime", newWorkDay.StartTime)
                .Set("WorkDays.$.EndTime", newWorkDay.EndTime);

            await dbSet.UpdateOneAsync(filter, update);
        }

        public async Task UpdateWorkDaysAsync(string scheduleId, List<WorkDay> workDays)
        {
            var scheduleFilter = Builders<Schedule>.Filter.Eq(x => x.Id, scheduleId);

            foreach (var workDay in workDays)
            {
                var workDayFilter = Builders<Schedule>.Filter.And(
                    scheduleFilter,
                    Builders<Schedule>.Filter.ElemMatch(x => x.WorkDays, wd => wd.Day == workDay.Day));

                var update = Builders<Schedule>.Update
                    .Set("WorkDays.$.StartTime", workDay.StartTime)
                    .Set("WorkDays.$.EndTime", workDay.EndTime);

                await dbSet.UpdateOneAsync(workDayFilter, update);
            }
        }

        public async Task<UpdateResult?> ClearMonthSchedule(string scheduleId)
        {
            var filter = Builders<Schedule>.Filter.Eq(x => x.Id, scheduleId);
            var update = Builders<Schedule>.Update.Set(x => x.WorkDays, new List<WorkDay>());

            var result = await dbSet.UpdateOneAsync(filter, update);

            return result;
        }

        public async Task DeleteScheduleAsync(string scheduleId)
        {
            var filter = Builders<Schedule>.Filter.Eq(x => x.Id, scheduleId);
            var result = await dbSet.DeleteOneAsync(filter);
        }
    }
}
