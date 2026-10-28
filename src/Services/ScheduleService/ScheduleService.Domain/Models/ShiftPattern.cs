namespace ScheduleService.Domain.Models;

public enum ShiftPattern
{
    Manual = 0,
    FirstShiftOnly = 1,
    SecondShiftOnly = 2,
    AlternateByWeekdayA = 3,
    AlternateByWeekdayB = 4,
    AlternateByMonthDayA = 5,
    AlternateByMonthDayB = 6,
}
