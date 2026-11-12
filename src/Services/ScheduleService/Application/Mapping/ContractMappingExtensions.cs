namespace ScheduleService.Application.Mapping;

using System.Globalization;
using ScheduleService.Application.Contracts;
using ScheduleService.Domain.Models;

public static class ContractMappingExtensions
{
    public static ScheduleDto ToDto(this MonthlySchedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        return new ScheduleDto(
            schedule.Id,
            schedule.Period.ToString("MMMM", CultureInfo.InvariantCulture).ToLowerInvariant(),
            schedule.WorkShifts
                .OrderBy(shift => shift.WorkDate)
                .Select(shift => shift.ToDto())
                .ToList());
    }

    public static WorkDayDto ToDto(this WorkShift shift)
    {
        ArgumentNullException.ThrowIfNull(shift);

        return new WorkDayDto
        {
            Day = shift.WorkDate.Day,
            StartTime = shift.StartsAtUtc,
            EndTime = shift.EndsAtUtc,
        };
    }

    public static ScheduleRulesDto ToDto(this ScheduleGenerationRule rule, string scheduleId)
    {
        ArgumentNullException.ThrowIfNull(rule);

        return new ScheduleRulesDto
        {
            Id = rule.Id,
            UserId = rule.UserId,
            DepartmentId = rule.DepartmentId,
            MonthName = rule.Period.ToString("MMMM", CultureInfo.InvariantCulture).ToLowerInvariant(),
            Year = rule.Period.Year,
            HoursPerMonth = rule.TargetMinutes / 60f,
            MaxHoursPerDay = rule.MaxShiftMinutes / 60f,
            StartWorkDayTime = rule.FirstShiftStart,
            OnlyFirstShift = rule.Pattern == ShiftPattern.FirstShiftOnly,
            OnlySecondShift = rule.Pattern == ShiftPattern.SecondShiftOnly,
            EvenDOW = rule.Pattern == ShiftPattern.AlternateByWeekdayA,
            UnEvenDOW = rule.Pattern == ShiftPattern.AlternateByWeekdayB,
            EvenDOM = rule.Pattern == ShiftPattern.AlternateByMonthDayA,
            UnEvenDOM = rule.Pattern == ShiftPattern.AlternateByMonthDayB,
            ScheduleId = scheduleId,
        };
    }

    public static CalendarDto ToDto(this CalendarException calendarException)
    {
        ArgumentNullException.ThrowIfNull(calendarException);

        return new CalendarDto
        {
            Id = calendarException.Id,
            Year = calendarException.HolidayDate.Year,
            HolidayDate = calendarException.HolidayDate,
            TransferDate = calendarException.TransferDate,
            MonthOfTransferDay = calendarException.TransferDate?.Month ?? 0,
            MonthOfHoliday = calendarException.HolidayDate.Month,
            HolidayDayOfMonth = calendarException.HolidayDate.Day,
            DayOfWeek = calendarException.HolidayDate.DayOfWeek,
        };
    }
}
