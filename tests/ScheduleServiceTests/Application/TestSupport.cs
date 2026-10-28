namespace Application;

internal static class TestSupport
{
    public static readonly DateOnly Period = new(2026, 10, 1);

    public static Mock<ScheduleDbContext> CreateDbContextMock() =>
        new(new DbContextOptionsBuilder<ScheduleDbContext>().Options);

    public static MonthlySchedule CreateSchedule(string id = "schedule-id") =>
        new(id, "user-id", "department-id", Period, Utc(2026, 9, 1));

    public static ScheduleGenerationRule CreateRule(
        string id = "rule-id",
        ShiftPattern pattern = ShiftPattern.FirstShiftOnly,
        int targetMinutes = 480,
        int maxShiftMinutes = 480) =>
        new(
            id,
            "user-id",
            "department-id",
            Period,
            targetMinutes,
            maxShiftMinutes,
            new TimeOnly(8, 0),
            new TimeOnly(14, 30),
            pattern);

    public static WorkShift CreateShift(
        string scheduleId = "schedule-id",
        int day = 1,
        int startHour = 8,
        int endHour = 16) =>
        new(
            Guid.NewGuid(),
            scheduleId,
            new DateOnly(2026, 10, day),
            Utc(2026, 10, day, startHour),
            Utc(2026, 10, day, endHour));

    public static DateTime Utc(int year, int month, int day, int hour = 0, int minute = 0) =>
        new(year, month, day, hour, minute, 0, DateTimeKind.Utc);
}
