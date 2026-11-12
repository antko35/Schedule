namespace ScheduleService.Application.Contracts;

public sealed class WorkDayDto
{
    public int Day { get; init; }

    public DateTime StartTime { get; init; }

    public DateTime EndTime { get; init; }
}
