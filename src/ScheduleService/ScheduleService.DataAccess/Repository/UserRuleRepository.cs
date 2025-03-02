using MongoDB.Driver;
using ScheduleService.DataAccess.Database;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using static System.Net.Mime.MediaTypeNames;

namespace ScheduleService.DataAccess.Repository
{
    public class UserRuleRepository : GenericRepository<UserScheduleRules>, IUserRuleRepository
    {
        public UserRuleRepository(DbContext context, DbOptions options)
            : base(context, options.UserScheduleRuleCollection)
        {
        }

        public async Task<IEnumerable<UserScheduleRules>?> GetUsersRulesByDepartment(string departmentId, string month, int year)
        {
            var filter = Builders<UserScheduleRules>.Filter.And(
                Builders<UserScheduleRules>.Filter.Eq(x => x.DepartmentId, departmentId),
                Builders<UserScheduleRules>.Filter.Eq(x => x.MonthName, month),
                Builders<UserScheduleRules>.Filter.Eq(x => x.Year, year));

            var rules = await dbSet.Find(filter).ToListAsync();

            return rules;
        }

        public async Task<string> GetIdByScheduleId(string scheduleId)
        {
            var filter = Builders<UserScheduleRules>.Filter.Eq(x => x.ScheduleId, scheduleId);

            var result = await dbSet.Find(filter).FirstOrDefaultAsync();

            return result.Id;
        }

        public async Task<List<UserScheduleRules>?> GetAllRulesByMonth(string month)
        {
            var filter = Builders<UserScheduleRules>.Filter.Eq(x => x.MonthName, month);

            var result = await dbSet.Find(filter).ToListAsync();

            return result;
        }

        public async Task<IEnumerable<UserScheduleRules>> GetAllByIds(string userId, string departmentId)
        {
            var filter = Builders<UserScheduleRules>.Filter.And(
                Builders<UserScheduleRules>.Filter.Eq(x => x.UserId, userId),
                Builders<UserScheduleRules>.Filter.Eq(x => x.DepartmentId, departmentId));

            var result = await dbSet.Find(filter).ToListAsync();

            return result;
        }

        public async Task DeleteAll(string userId, string departmentId)
        {
            var filter = Builders<UserScheduleRules>.Filter.And(
                    Builders<UserScheduleRules>.Filter.Eq(x => x.UserId, userId),
                    Builders<UserScheduleRules>.Filter.Eq(x => x.DepartmentId, departmentId));

            dbSet.DeleteManyAsync(filter);
        }

        public async Task<UserScheduleRules> GetMonthScheduleRules(string userId, string departmentId, string monthName, int year)
        {
            var filter = Builders<UserScheduleRules>.Filter.And(
               Builders<UserScheduleRules>.Filter.Eq(x => x.UserId, userId),
               Builders<UserScheduleRules>.Filter.Eq(x => x.DepartmentId, departmentId),
               Builders<UserScheduleRules>.Filter.Eq(x => x.MonthName, monthName),
               Builders<UserScheduleRules>.Filter.Eq(x => x.Year, year));

            var result = await dbSet.Find(filter).FirstOrDefaultAsync();

            return result;
        }
    }
}
