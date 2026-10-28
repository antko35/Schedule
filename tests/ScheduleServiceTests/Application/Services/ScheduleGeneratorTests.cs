namespace Application.Services;

using FluentAssertions;
using ScheduleService.Application.Services;
using ScheduleService.Domain.Models;
using Xunit;

public sealed class ScheduleGeneratorTests
{
    [Fact]
    public void GenerateWorkShiftsForUser_ShouldSkipOrdinaryWeekends()
    {
        var rule = CreateRule(1_440, 480, ShiftPattern.FirstShiftOnly);

        var result = ScheduleGenerator.GenerateWorkShiftsForUser(rule, [], [], "schedule-id");

        result.Select(shift => shift.WorkDate.Day).Should().Equal(1, 2, 5);
    }

    [Fact]
    public void GenerateWorkShiftsForUser_ShouldIncludeTransferredSaturday()
    {
        var rule = CreateRule(960, 480, ShiftPattern.FirstShiftOnly);
        var transfer = new CalendarException(
            "transfer-id",
            new DateOnly(2026, 10, 2),
            new DateOnly(2026, 10, 3));

        var result = ScheduleGenerator.GenerateWorkShiftsForUser(
            rule,
            [transfer],
            [transfer],
            "schedule-id");

        result.Select(shift => shift.WorkDate.Day).Should().Equal(1, 3);
    }

    [Theory]
    [InlineData(ShiftPattern.AlternateByWeekdayA, 8, 0)]
    [InlineData(ShiftPattern.AlternateByWeekdayB, 14, 30)]
    public void GenerateWorkShiftsForUser_ShouldApplyWeekdayPattern(
        ShiftPattern pattern,
        int expectedHour,
        int expectedMinute)
    {
        var rule = CreateRule(240, 240, pattern);

        var result = ScheduleGenerator.GenerateWorkShiftsForUser(rule, [], [], "schedule-id");

        result.Should().ContainSingle();
        result[0].StartsAtUtc.Hour.Should().Be(expectedHour);
        result[0].StartsAtUtc.Minute.Should().Be(expectedMinute);
    }

    [Fact]
    public void GenerateWorkShiftsForUser_ShouldRejectManualPattern()
    {
        var rule = CreateRule(480, 240, ShiftPattern.Manual);

        var action = () => ScheduleGenerator.GenerateWorkShiftsForUser(rule, [], [], "schedule-id");

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*shift pattern*");
    }

    [Fact]
    public void GenerateWorkShiftsForUser_ShouldUseTargetAndMaximumShiftMinutes()
    {
        var rule = CreateRule(500, 240, ShiftPattern.FirstShiftOnly);

        var result = ScheduleGenerator.GenerateWorkShiftsForUser(rule, [], [], "schedule-id");

        result.Should().HaveCount(3);
        (result[0].EndsAtUtc - result[0].StartsAtUtc).Should().Be(TimeSpan.FromMinutes(240));
        (result[2].EndsAtUtc - result[2].StartsAtUtc).Should().Be(TimeSpan.FromMinutes(20));
        result.Sum(shift => (shift.EndsAtUtc - shift.StartsAtUtc).TotalMinutes).Should().Be(500);
    }

    [Fact]
    public void GenerateWorkShiftsForUser_ShouldRejectTargetThatCannotFitInMonth()
    {
        var rule = CreateRule(10_000, 240, ShiftPattern.FirstShiftOnly);

        var action = () => ScheduleGenerator.GenerateWorkShiftsForUser(rule, [], [], "schedule-id");

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot fit its monthly target*");
    }

    [Fact]
    public void GenerateWorkShiftsForUser_ShouldSkipHoliday()
    {
        var rule = CreateRule(480, 240, ShiftPattern.FirstShiftOnly);
        var holiday = new CalendarException("holiday-id", new DateOnly(2026, 10, 1));

        var result = ScheduleGenerator.GenerateWorkShiftsForUser(rule, [holiday], [], "schedule-id");

        result.Should().NotContain(shift => shift.WorkDate == holiday.HolidayDate);
        result[0].WorkDate.Should().Be(new DateOnly(2026, 10, 2));
    }

    [Fact]
    public void GenerateWorkShiftsForUser_ShouldUseConfiguredSecondShiftStart()
    {
        var rule = CreateRule(240, 240, ShiftPattern.SecondShiftOnly);

        var result = ScheduleGenerator.GenerateWorkShiftsForUser(rule, [], [], "schedule-id");

        result.Should().ContainSingle();
        result[0].StartsAtUtc.Hour.Should().Be(14);
        result[0].StartsAtUtc.Minute.Should().Be(30);
        result[0].StartsAtUtc.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Theory]
    [InlineData(ShiftPattern.AlternateByMonthDayA, 2)]
    [InlineData(ShiftPattern.AlternateByMonthDayB, 1)]
    public void GenerateWorkShiftsForUser_ShouldApplyMonthDayPattern(
        ShiftPattern pattern,
        int expectedFirstShiftDay)
    {
        var rule = CreateRule(480, 240, pattern);

        var result = ScheduleGenerator.GenerateWorkShiftsForUser(rule, [], [], "schedule-id");

        result.Single(shift => shift.WorkDate.Day == expectedFirstShiftDay)
            .StartsAtUtc.Hour.Should().Be(8);
    }

    private static ScheduleGenerationRule CreateRule(
        int targetMinutes,
        int maxShiftMinutes,
        ShiftPattern pattern) =>
        new(
            "rule-id",
            "user-id",
            "department-id",
            new DateOnly(2026, 10, 1),
            targetMinutes,
            maxShiftMinutes,
            new TimeOnly(8, 0),
            new TimeOnly(14, 30),
            pattern);
}
