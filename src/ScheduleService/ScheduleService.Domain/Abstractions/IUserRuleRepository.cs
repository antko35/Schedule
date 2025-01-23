using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;

namespace ScheduleService.DataAccess.Repository
{
    public interface IUserRuleRepository : IGenericRepository<UserScheduleRules>
    {
        Task AddWorkDayAsync(string userId, string departmentId, string month, WorkDay workDay);

        Task<UserScheduleRules> GetWorkDaySchedue(string userId, string departmentId, string monthName);

        Task<IEnumerable<UserScheduleRules>> GetUsersRulesByDepartment( string departmentId, string month);
        Task UpdateWorkDayAsync(string userId, string departmentId, string month, WorkDay updatedWorkDay);
        Task DeleteWorkDayAsync(string userId, string departmentId, string month, DateTime workDayToDelete);
    }
}