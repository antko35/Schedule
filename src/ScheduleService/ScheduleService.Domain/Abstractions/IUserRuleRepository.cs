using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;

namespace ScheduleService.DataAccess.Repository
{
    public interface IUserRuleRepository : IGenericRepository<UserScheduleRules>
    {
        Task<UserScheduleRules> GetMonthScheduleRules(string userId, string departmentId, string monthName, int year);

        Task<IEnumerable<UserScheduleRules>> GetUsersRulesByDepartment( string departmentId, string month, int year);
    }
}