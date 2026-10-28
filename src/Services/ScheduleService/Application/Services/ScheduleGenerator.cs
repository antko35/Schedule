namespace ScheduleService.Application.Services;

using ScheduleService.Domain.Models;

public static class ScheduleGenerator
{
    public static IReadOnlyList<WorkShift> GenerateWorkShiftsForUser(
        ScheduleGenerationRule rule,
        IEnumerable<CalendarException> officialHolidays,
        IEnumerable<CalendarException> transferDays,
        string monthlyScheduleId)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(officialHolidays);
        ArgumentNullException.ThrowIfNull(transferDays);
        ArgumentException.ThrowIfNullOrWhiteSpace(monthlyScheduleId);

        if (rule.TargetMinutes <= 0)
        {
            throw new InvalidOperationException($"Rule {rule.Id} must define positive monthly hours.");
        }

        if (rule.MaxShiftMinutes <= 0)
        {
            throw new InvalidOperationException($"Rule {rule.Id} must define positive daily hours.");
        }

        if (rule.Pattern == ShiftPattern.Manual)
        {
            throw new InvalidOperationException($"Rule {rule.Id} must define a shift pattern before generation.");
        }

        var holidays = officialHolidays.Select(item => item.HolidayDate).ToHashSet();
        var transfers = transferDays
            .Where(item => item.TransferDate.HasValue)
            .GroupBy(item => item.TransferDate!.Value)
            .ToDictionary(group => group.Key, group => group.First().HolidayDate);
        var shifts = new List<WorkShift>();
        var remainingMinutes = rule.TargetMinutes;
        var daysInMonth = DateTime.DaysInMonth(rule.Period.Year, rule.Period.Month);

        for (var day = 1; day <= daysInMonth && remainingMinutes > 0; day++)
        {
            var workDate = new DateOnly(rule.Period.Year, rule.Period.Month, day);
            if (holidays.Contains(workDate))
            {
                continue;
            }

            var effectiveDayOfWeek = workDate.DayOfWeek;
            if (transfers.TryGetValue(workDate, out var replacedDate))
            {
                if (replacedDate.Month != rule.Period.Month)
                {
                    continue;
                }

                effectiveDayOfWeek = replacedDate.DayOfWeek;
            }
            else if (effectiveDayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                continue;
            }

            var startsAt = IsFirstShift(rule.Pattern, effectiveDayOfWeek, day)
                ? rule.FirstShiftStart
                : rule.SecondShiftStart;
            var durationMinutes = Math.Min(rule.MaxShiftMinutes, remainingMinutes);
            var startsAtUtc = CreateUtc(rule.Period.Year, rule.Period.Month, day, startsAt);
            var endsAtUtc = startsAtUtc.AddMinutes(durationMinutes);

            shifts.Add(new WorkShift(
                Guid.NewGuid(),
                monthlyScheduleId,
                workDate,
                startsAtUtc,
                endsAtUtc));
            remainingMinutes -= durationMinutes;
        }

        if (remainingMinutes > 0)
        {
            throw new InvalidOperationException(
                $"Rule {rule.Id} cannot fit its monthly target within the available workdays and maximum shift length.");
        }

        return shifts;
    }

    private static bool IsFirstShift(ShiftPattern pattern, DayOfWeek dayOfWeek, int dayOfMonth) =>
        pattern switch
        {
            ShiftPattern.FirstShiftOnly => true,
            ShiftPattern.SecondShiftOnly => false,
            ShiftPattern.AlternateByWeekdayA => dayOfWeek is DayOfWeek.Tuesday or DayOfWeek.Thursday,
            ShiftPattern.AlternateByWeekdayB => dayOfWeek is DayOfWeek.Monday or DayOfWeek.Wednesday or DayOfWeek.Friday,
            ShiftPattern.AlternateByMonthDayA => dayOfMonth % 2 == 0,
            ShiftPattern.AlternateByMonthDayB => dayOfMonth % 2 != 0,
            ShiftPattern.Manual => false,
            _ => throw new ArgumentOutOfRangeException(nameof(pattern), pattern, "Unknown shift pattern."),
        };

    private static DateTime CreateUtc(int year, int month, int day, TimeOnly time) =>
        new(year, month, day, time.Hour, time.Minute, time.Second, DateTimeKind.Utc);
}
