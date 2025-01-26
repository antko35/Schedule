using ScheduleService.Domain.Models;

namespace ScheduleService.Domain.Abstractions
{
    public interface IScheduleRepository : IGenericRepository<Schedule>
    {
        Task AddWorkDayAsync(string scheduleId, WorkDay newWorkDay);

        Task<Schedule?> GetWorkDayAsync(string scheduleId, int day);

        Task UpdateWorkDayAsync(string scheduleId, WorkDay newWorkDay);
    }
}