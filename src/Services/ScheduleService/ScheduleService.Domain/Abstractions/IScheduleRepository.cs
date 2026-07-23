namespace ScheduleService.Domain.Abstractions
{
    using MongoDB.Driver;
    using ScheduleService.Domain.Models;

    public interface IScheduleRepository : IGenericRepository<Schedule>
    {
        Task AddWorkDayAsync(string scheduleId, WorkDay newWorkDay);

        Task<UpdateResult?> ClearMonthSchedule(string scheduleId);

        Task DeleteWorkDayAsync(string scheduleId, int day);

        Task<Schedule> GetMonthSchedule(string scheduleId);

        Task<Schedule?> GetWorkDayAsync(string scheduleId, int day);

        Task UpdateWorkDayAsync(string scheduleId, WorkDay newWorkDay);

        Task UpdateWorkDaysAsync(string scheduleId, List<WorkDay> workDays);

        Task<List<Schedule>?> GetEmptySchedules(int year, string month);

        Task DeleteScheduleAsync(string scheduleId);
    }
}