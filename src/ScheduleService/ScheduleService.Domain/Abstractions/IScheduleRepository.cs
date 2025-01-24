using ScheduleService.Domain.Models;

namespace ScheduleService.Domain.Abstractions
{
    public interface IScheduleRepository : IGenericRepository<Schedule>
    {
        Task AddWorkDayAsync(string scheduleId, WorkDay newWorkDay);
        Task<WorkDay> GetWorkDayAsync(string scheduleId, int day);
    }
}