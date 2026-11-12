namespace ScheduleService.DataAccess.Repository;

using Microsoft.EntityFrameworkCore;
using ScheduleService.DataAccess.Database;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;

public sealed class CalendarRepository : ICalendarRepository
{
    private readonly ScheduleDbContext context;

    public CalendarRepository(ScheduleDbContext context)
    {
        this.context = context;
    }

    public async Task<IReadOnlyList<CalendarException>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.CalendarExceptions.OrderBy(item => item.HolidayDate).ToListAsync(cancellationToken);

    public Task<CalendarException?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return context.CalendarExceptions
            .SingleOrDefaultAsync(calendarException => calendarException.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<CalendarException>> GetMonthHolidaysAsync(
        DateOnly period,
        CancellationToken cancellationToken)
    {
        var nextPeriod = period.AddMonths(1);
        return await context.CalendarExceptions
            .AsNoTracking()
            .Where(calendarException =>
                calendarException.HolidayDate >= period
                && calendarException.HolidayDate < nextPeriod)
            .OrderBy(calendarException => calendarException.HolidayDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CalendarException>> GetMonthTransferDaysAsync(
        DateOnly period,
        CancellationToken cancellationToken)
    {
        var nextPeriod = period.AddMonths(1);
        return await context.CalendarExceptions
            .AsNoTracking()
            .Where(calendarException =>
                calendarException.TransferDate >= period
                && calendarException.TransferDate < nextPeriod)
            .OrderBy(calendarException => calendarException.TransferDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CalendarException>> GetYearHolidaysAsync(
        int year,
        CancellationToken cancellationToken)
    {
        var firstDay = new DateOnly(year, 1, 1);
        var nextYear = firstDay.AddYears(1);
        return await context.CalendarExceptions
            .AsNoTracking()
            .Where(calendarException =>
                calendarException.HolidayDate >= firstDay
                && calendarException.HolidayDate < nextYear)
            .OrderBy(calendarException => calendarException.HolidayDate)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(CalendarException calendarException, CancellationToken cancellationToken)
    {
        return context.CalendarExceptions.AddAsync(calendarException, cancellationToken).AsTask();
    }

    public void Remove(CalendarException calendarException)
    {
        context.CalendarExceptions.Remove(calendarException);
    }
}
