using ScheduleService.Domain.Models;

namespace ScheduleService.Domain.Abstractions
{
    public interface ICalendarRepository : IGenericRepository<Calendar>
    {
        Task<List<Calendar>> GetMonthHolidays(int year, int month);

        Task<List<Calendar>> GetMonthTransferDays(int year, int month);
        Task<List<Calendar>> GetYearHolidays(int year);
    }
}