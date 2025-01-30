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

        public async Task<UserScheduleRules> GetMonthScheduleRules(string userId, string departmentId, string monthName, int year)
        {
            var filter = Builders<UserScheduleRules>.Filter.And(
               Builders<UserScheduleRules>.Filter.Eq(x => x.UserId, userId),
               Builders<UserScheduleRules>.Filter.Eq(x => x.DepartmentId, departmentId),
               Builders<UserScheduleRules>.Filter.Eq(x => x.Month, monthName));

            var result = await dbSet.Find(filter).FirstOrDefaultAsync();

            return result;
        }
    }
}
