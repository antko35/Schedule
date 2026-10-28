namespace ScheduleService.Domain.Abstractions;

using ScheduleService.Domain.Models;

public interface ICalendarRepository
{
    Task<IReadOnlyList<CalendarException>> GetAllAsync(CancellationToken cancellationToken);

    Task<CalendarException?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task<IReadOnlyList<CalendarException>> GetMonthHolidaysAsync(
        DateOnly period,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CalendarException>> GetMonthTransferDaysAsync(
        DateOnly period,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CalendarException>> GetYearHolidaysAsync(
        int year,
        CancellationToken cancellationToken);

    Task AddAsync(CalendarException calendarException, CancellationToken cancellationToken);

    void Remove(CalendarException calendarException);
}
