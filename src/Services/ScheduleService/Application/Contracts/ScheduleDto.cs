namespace ScheduleService.Application.Contracts;

public sealed record ScheduleDto(
    string Id,
    string MonthName,
    IReadOnlyList<WorkDayDto> WorkDays);
