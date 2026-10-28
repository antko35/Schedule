namespace ScheduleService.Domain.Models;

public sealed class ScheduleGenerationRule : Entity
{
    public ScheduleGenerationRule(
        string id,
        string userId,
        string departmentId,
        DateOnly period,
        int targetMinutes,
        int maxShiftMinutes,
        TimeOnly firstShiftStart,
        TimeOnly secondShiftStart,
        ShiftPattern pattern)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(departmentId);
        EnsureFirstDayOfMonth(period);
        EnsurePositive(targetMinutes, nameof(targetMinutes));
        EnsurePositive(maxShiftMinutes, nameof(maxShiftMinutes));
        EnsureDefinedPattern(pattern);

        UserId = userId;
        DepartmentId = departmentId;
        Period = period;
        TargetMinutes = targetMinutes;
        MaxShiftMinutes = maxShiftMinutes;
        FirstShiftStart = firstShiftStart;
        SecondShiftStart = secondShiftStart;
        Pattern = pattern;
    }

#pragma warning disable SA1202 // EF Core requires a non-public parameterless constructor.
    private ScheduleGenerationRule()
    {
    }
#pragma warning restore SA1202

    public string UserId { get; private set; } = string.Empty;

    public string DepartmentId { get; private set; } = string.Empty;

    public DateOnly Period { get; private set; }

    public int TargetMinutes { get; private set; }

    public int MaxShiftMinutes { get; private set; }

    public TimeOnly FirstShiftStart { get; private set; }

    public TimeOnly SecondShiftStart { get; private set; }

    public ShiftPattern Pattern { get; private set; }

    public void Reconfigure(
        int targetMinutes,
        int maxShiftMinutes,
        TimeOnly firstShiftStart,
        TimeOnly secondShiftStart,
        ShiftPattern pattern)
    {
        EnsurePositive(targetMinutes, nameof(targetMinutes));
        EnsurePositive(maxShiftMinutes, nameof(maxShiftMinutes));
        EnsureDefinedPattern(pattern);

        TargetMinutes = targetMinutes;
        MaxShiftMinutes = maxShiftMinutes;
        FirstShiftStart = firstShiftStart;
        SecondShiftStart = secondShiftStart;
        Pattern = pattern;
    }

    private static void EnsureFirstDayOfMonth(DateOnly period)
    {
        if (period.Day != 1)
        {
            throw new ArgumentException("A rule period must be the first day of a month.", nameof(period));
        }
    }

    private static void EnsurePositive(int value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "The value must be positive.");
        }
    }

    private static void EnsureDefinedPattern(ShiftPattern pattern)
    {
        if (!Enum.IsDefined(pattern))
        {
            throw new ArgumentOutOfRangeException(nameof(pattern), pattern, "Unknown shift pattern.");
        }
    }
}
