namespace ScheduleService.Domain.Abstractions
{
    using MongoDB.Driver;
    using ScheduleService.Domain.Models;

    public interface IScheduleRepository : IGenericRepository<Schedule>
    {
        Task AddWorkDayAsync(string scheduleId, WorkDay newWorkDay);

        Task<UpdateResult?> DeleteMonthSchedule(string scheduleId);

        Task DeleteWorkDayAsync(string scheduleId, int day);

        Task<Schedule> GetMonthSchedule(string scheduleId);

        Task<Schedule?> GetWorkDayAsync(string scheduleId, int day);

        Task UpdateWorkDayAsync(string scheduleId, WorkDay newWorkDay);
    }
}