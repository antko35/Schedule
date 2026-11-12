namespace ScheduleService.Domain.Models;

public sealed class MonthlySchedule : Entity
{
    private readonly List<WorkShift> workShifts = new List<WorkShift>();

    public MonthlySchedule(
        string id,
        string userId,
        string departmentId,
        DateOnly period,
        DateTime createdAtUtc)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(departmentId);
        EnsureFirstDayOfMonth(period);
        EnsureUtc(createdAtUtc, nameof(createdAtUtc));

        UserId = userId;
        DepartmentId = departmentId;
        Period = period;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

#pragma warning disable SA1202 // EF Core requires a non-public parameterless constructor.
    private MonthlySchedule()
    {
    }
#pragma warning restore SA1202

    public string UserId { get; private set; } = string.Empty;

    public string DepartmentId { get; private set; } = string.Empty;

    public DateOnly Period { get; private set; }

    public IReadOnlyCollection<WorkShift> WorkShifts => workShifts.AsReadOnly();

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public void SetShift(WorkShift shift, DateTime updatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(shift);
        EnsureShiftBelongsToSchedule(shift);
        EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));

        var existingShift = workShifts.SingleOrDefault(item => item.WorkDate == shift.WorkDate);
        if (existingShift is not null)
        {
            existingShift.Reschedule(shift.WorkDate, shift.StartsAtUtc, shift.EndsAtUtc);
            UpdatedAtUtc = updatedAtUtc;
            return;
        }

        workShifts.Add(shift);
        UpdatedAtUtc = updatedAtUtc;
    }

    public bool RemoveShift(DateOnly workDate, DateTime updatedAtUtc)
    {
        EnsureDateBelongsToPeriod(workDate);
        EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));

        var removed = workShifts.RemoveAll(item => item.WorkDate == workDate) > 0;
        if (removed)
        {
            UpdatedAtUtc = updatedAtUtc;
        }

        return removed;
    }

    public void ReplaceShifts(IEnumerable<WorkShift> shifts, DateTime updatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(shifts);
        EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));

        var replacement = shifts.ToList();
        foreach (var shift in replacement)
        {
            EnsureShiftBelongsToSchedule(shift);
        }

        var duplicateDate = replacement
            .GroupBy(shift => shift.WorkDate)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateDate is not null)
        {
            throw new ArgumentException(
                $"Only one shift is allowed on {duplicateDate.Key:yyyy-MM-dd}.",
                nameof(shifts));
        }

        workShifts.Clear();
        workShifts.AddRange(replacement);
        UpdatedAtUtc = updatedAtUtc;
    }

    private static void EnsureFirstDayOfMonth(DateOnly period)
    {
        if (period.Day != 1)
        {
            throw new ArgumentException("A schedule period must be the first day of a month.", nameof(period));
        }
    }

    private static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The timestamp must be in UTC.", parameterName);
        }
    }

    private void EnsureShiftBelongsToSchedule(WorkShift shift)
    {
        if (!string.Equals(shift.MonthlyScheduleId, Id, StringComparison.Ordinal))
        {
            throw new ArgumentException("The shift belongs to another schedule.", nameof(shift));
        }

        EnsureDateBelongsToPeriod(shift.WorkDate);
    }

    private void EnsureDateBelongsToPeriod(DateOnly workDate)
    {
        if (workDate.Year != Period.Year || workDate.Month != Period.Month)
        {
            throw new ArgumentOutOfRangeException(
                nameof(workDate),
                workDate,
                "The work date must be inside the schedule period.");
        }
    }
}
