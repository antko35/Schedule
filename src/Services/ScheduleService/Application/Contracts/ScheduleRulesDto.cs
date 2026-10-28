namespace ScheduleService.Application.Contracts;

public sealed class ScheduleRulesDto
{
    public string Id { get; init; } = string.Empty;

    public string UserId { get; init; } = string.Empty;

    public string DepartmentId { get; init; } = string.Empty;

    public string MonthName { get; init; } = string.Empty;

    public int Year { get; init; }

    public float HoursPerMonth { get; init; }

    public float MaxHoursPerDay { get; init; }

    public TimeOnly StartWorkDayTime { get; init; }

    public bool OnlyFirstShift { get; init; }

    public bool OnlySecondShift { get; init; }

    public bool EvenDOW { get; init; }

    public bool UnEvenDOW { get; init; }

    public bool EvenDOM { get; init; }

    public bool UnEvenDOM { get; init; }

    public string ScheduleId { get; init; } = string.Empty;
}
