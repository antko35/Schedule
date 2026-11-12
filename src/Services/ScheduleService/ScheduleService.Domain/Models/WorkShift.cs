namespace ScheduleService.Domain.Models;

public sealed class WorkShift
{
    public WorkShift(
        Guid id,
        string monthlyScheduleId,
        DateOnly workDate,
        DateTime startsAtUtc,
        DateTime endsAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A shift identifier cannot be empty.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(monthlyScheduleId);
        EnsureUtc(startsAtUtc, nameof(startsAtUtc));
        EnsureUtc(endsAtUtc, nameof(endsAtUtc));
        EnsureValidInterval(startsAtUtc, endsAtUtc);

        Id = id;
        MonthlyScheduleId = monthlyScheduleId;
        WorkDate = workDate;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
    }

#pragma warning disable SA1202 // EF Core requires a non-public parameterless constructor.
    private WorkShift()
    {
    }
#pragma warning restore SA1202

    public Guid Id { get; private set; }

    public string MonthlyScheduleId { get; private set; } = string.Empty;

    public DateOnly WorkDate { get; private set; }

    public DateTime StartsAtUtc { get; private set; }

    public DateTime EndsAtUtc { get; private set; }

    public void Reschedule(DateOnly workDate, DateTime startsAtUtc, DateTime endsAtUtc)
    {
        EnsureUtc(startsAtUtc, nameof(startsAtUtc));
        EnsureUtc(endsAtUtc, nameof(endsAtUtc));
        EnsureValidInterval(startsAtUtc, endsAtUtc);

        WorkDate = workDate;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
    }

    private static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("The timestamp must be in UTC.", parameterName);
        }
    }

    private static void EnsureValidInterval(DateTime startsAtUtc, DateTime endsAtUtc)
    {
        if (endsAtUtc <= startsAtUtc)
        {
            throw new ArgumentException("A shift must end after it starts.", nameof(endsAtUtc));
        }
    }
}
