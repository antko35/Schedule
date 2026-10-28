namespace Application.Contracts;

using System.Text.Json;
using ScheduleService.Application.Contracts;

public sealed class SerializationContractTests
{
    private static readonly JsonSerializerOptions WebOptions = JsonSerializerOptions.Web;

    [Fact]
    public void ScheduleDto_PreservesLegacyCamelCaseShapeAndUtcTimestamps()
    {
        var dto = new ScheduleDto(
            "schedule-id",
            "october",
            [
                new WorkDayDto
                {
                    Day = 5,
                    StartTime = new DateTime(2026, 10, 5, 8, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2026, 10, 5, 16, 0, 0, DateTimeKind.Utc),
                },
            ]);

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(dto, WebOptions));
        var root = document.RootElement;

        root.GetProperty("id").GetString().Should().Be("schedule-id");
        root.GetProperty("monthName").GetString().Should().Be("october");
        var workDay = root.GetProperty("workDays")[0];
        workDay.GetProperty("day").GetInt32().Should().Be(5);
        workDay.GetProperty("startTime").GetString().Should().Be("2026-10-05T08:00:00Z");
        workDay.GetProperty("endTime").GetString().Should().Be("2026-10-05T16:00:00Z");
    }

    [Fact]
    public void ScheduleRulesDto_PreservesLegacyFieldNames()
    {
        var dto = new ScheduleRulesDto
        {
            Id = "rule-id",
            UserId = "user-id",
            DepartmentId = "department-id",
            MonthName = "october",
            Year = 2026,
            HoursPerMonth = 132,
            MaxHoursPerDay = 6.5f,
            StartWorkDayTime = new TimeOnly(8, 0),
            OnlyFirstShift = true,
            ScheduleId = "schedule-id",
        };

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(dto, WebOptions));
        var root = document.RootElement;

        root.GetProperty("userId").GetString().Should().Be("user-id");
        root.GetProperty("departmentId").GetString().Should().Be("department-id");
        root.GetProperty("hoursPerMonth").GetSingle().Should().Be(132);
        root.GetProperty("maxHoursPerDay").GetSingle().Should().Be(6.5f);
        root.GetProperty("startWorkDayTime").GetString().Should().Be("08:00:00");
        root.GetProperty("onlyFirstShift").GetBoolean().Should().BeTrue();
        root.GetProperty("scheduleId").GetString().Should().Be("schedule-id");
    }

    [Fact]
    public void CalendarDto_SerializesDateOnlyAsIsoDate()
    {
        var dto = new CalendarDto
        {
            Id = "holiday-id",
            Year = 2026,
            HolidayDate = new DateOnly(2026, 10, 5),
            TransferDate = new DateOnly(2026, 10, 10),
            MonthOfTransferDay = 10,
            MonthOfHoliday = 10,
            HolidayDayOfMonth = 5,
            DayOfWeek = DayOfWeek.Monday,
        };

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(dto, WebOptions));
        var root = document.RootElement;

        root.GetProperty("holidayDate").GetString().Should().Be("2026-10-05");
        root.GetProperty("transferDate").GetString().Should().Be("2026-10-10");
        root.GetProperty("holidayDayOfMonth").GetInt32().Should().Be(5);
        root.GetProperty("dayOfWeek").GetInt32().Should().Be((int)DayOfWeek.Monday);
    }
}
