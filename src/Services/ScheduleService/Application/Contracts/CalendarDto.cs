namespace ScheduleService.Application.Contracts;

public sealed class CalendarDto
{
    public string Id { get; init; } = string.Empty;

    public int Year { get; init; }

    public DateOnly HolidayDate { get; init; }

    public DateOnly? TransferDate { get; init; }

    public int MonthOfTransferDay { get; init; }

    public int MonthOfHoliday { get; init; }

    public int HolidayDayOfMonth { get; init; }

    public DayOfWeek DayOfWeek { get; init; }
}
