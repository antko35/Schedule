using MongoDB.Driver;
using ScheduleService.DataAccess.Database;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ScheduleService.DataAccess.Repository
{
    public class UserRuleRepository : GenericRepository<UserScheduleRules>, IUserRuleRepository
    {
        public UserRuleRepository(DbContext context, DbOptions options)
            : base(context, options.UserScheduleRuleCollection)
        {
        }

        public async Task<IEnumerable<UserScheduleRules>?> GetUsersRulesByDepartment(string departmentId, string month)
        {
            var filter = Builders<UserScheduleRules>.Filter.And(
                Builders<UserScheduleRules>.Filter.Eq(x => x.DepartmentId, departmentId),
                Builders<UserScheduleRules>.Filter.Eq(x => x.Month, month));

            var rules = await dbSet.Find(filter).ToListAsync();

            return rules;
        }

        public async Task AddWorkDayAsync(string userId, string departmentId, string month, WorkDay workDay)
        {
            var filter = Builders<UserScheduleRules>.Filter.And(
               Builders<UserScheduleRules>.Filter.Eq(x => x.UserId, userId),
               Builders<UserScheduleRules>.Filter.Eq(x => x.DepartmentId, departmentId),
               Builders<UserScheduleRules>.Filter.Eq(x => x.Month, month));

            var update = Builders<UserScheduleRules>.Update.Push(s => s.Schedule, workDay);

            var result = await dbSet.UpdateOneAsync(filter, update);
        }

        public async Task<UserScheduleRules> GetMonthScheduleRules(string userId, string departmentId, string monthName)
        {
            var filter = Builders<UserScheduleRules>.Filter.And(
               Builders<UserScheduleRules>.Filter.Eq(x => x.UserId, userId),
               Builders<UserScheduleRules>.Filter.Eq(x => x.DepartmentId, departmentId),
               Builders<UserScheduleRules>.Filter.Eq(x => x.Month, monthName));

            var result = await dbSet.Find(filter).FirstOrDefaultAsync();

            return result;
        }

        public async Task UpdateWorkDayAsync(string userId, string departmentId, string month, WorkDay updatedWorkDay)
        {
            var filter = Builders<UserScheduleRules>.Filter.And(
                Builders<UserScheduleRules>.Filter.Eq(x => x.UserId, userId),
                Builders<UserScheduleRules>.Filter.Eq(x => x.DepartmentId, departmentId),
                Builders<UserScheduleRules>.Filter.Eq(x => x.Month, month),
                Builders<UserScheduleRules>.Filter.ElemMatch(x => x.Schedule, day => day.StartTime.Day == updatedWorkDay.StartTime.Day));

            var update = Builders<UserScheduleRules>.Update.Set(x => x.Schedule[-1], updatedWorkDay);

            var result = await dbSet.UpdateOneAsync(filter, update);
        }

        public async Task DeleteWorkDayAsync(string userId, string departmentId, string month, DateTime workDayToDelete)
        {
            var filter = Builders<UserScheduleRules>.Filter.And(
                Builders<UserScheduleRules>.Filter.Eq(x => x.UserId, userId),
                Builders<UserScheduleRules>.Filter.Eq(x => x.DepartmentId, departmentId),
                Builders<UserScheduleRules>.Filter.Eq(x => x.Month, month));

            var update = Builders<UserScheduleRules>.Update.PullFilter(
                x => x.Schedule,
                day => day.StartTime.Day == workDayToDelete.Day);

            var result = await dbSet.UpdateOneAsync(filter, update);
        }

        public async Task<WorkDay?> GetWorkDayAsync(string userId, string departmentId, string monthName, int day)
        {
            var filter = Builders<UserScheduleRules>.Filter.And(
              Builders<UserScheduleRules>.Filter.Eq(x => x.UserId, userId),
              Builders<UserScheduleRules>.Filter.Eq(x => x.DepartmentId, departmentId),
              Builders<UserScheduleRules>.Filter.Eq(x => x.Month, monthName));

            var userScheduleRules = await dbSet.Find(filter).FirstOrDefaultAsync();

            var workDay = userScheduleRules.Schedule.FirstOrDefault(wd => wd.StartTime.Day == day);

            return workDay;
        }
    }
}
