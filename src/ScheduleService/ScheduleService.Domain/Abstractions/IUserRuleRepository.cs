using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;

namespace ScheduleService.DataAccess.Repository
{
    public interface IUserRuleRepository : IGenericRepository<UserScheduleRules>
    {
        Task<UserScheduleRules> GetMonthScheduleRules(string userId, string departmentId, string monthName, int year);

        Task<IEnumerable<UserScheduleRules>> GetUsersRulesByDepartment( string departmentId, string month, int year);

        Task<string> GetIdByScheduleId(string scheduleId);

        Task<List<UserScheduleRules>?> GetAllRulesByMonth(string month);

        Task<IEnumerable<UserScheduleRules>> GetAllByIds(string requestUserId, string requestDepartmentId);

        Task DeleteAll(string userId, string departmentId);
    }
}