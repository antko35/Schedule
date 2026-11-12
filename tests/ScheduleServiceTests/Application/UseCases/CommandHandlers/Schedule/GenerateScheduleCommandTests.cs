namespace Application.UseCases.CommandHandlers.Schedule;
using ScheduleService.Application.UseCases.CommandHandlers.Schedule;
using ScheduleService.Application.UseCases.Commands.Schedule;
public sealed class GenerateScheduleCommandTests
{
    [Fact]
    public async Task ReplacesOnlySelectedScheduleAndCommitsOnce()
    {
        var period = new DateOnly(2025, 2, 1);
        var rule = new ScheduleGenerationRule("r", "u", "d", period, 160 * 60, 480, new TimeOnly(8, 0), new TimeOnly(14, 30), ShiftPattern.FirstShiftOnly);
        var schedule = new MonthlySchedule("s", "u", "d", period, DateTime.UtcNow);
        var rules = new Mock<IScheduleGenerationRuleRepository>();
        var schedules = new Mock<IScheduleRepository>();
        var calendar = new Mock<ICalendarRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        rules.Setup(x => x.GetAsync("u", "d", period, default)).ReturnsAsync(rule);
        schedules.Setup(x => x.GetAsync("u", "d", period, default)).ReturnsAsync(schedule);
        calendar.Setup(x => x.GetMonthHolidaysAsync(period, default)).ReturnsAsync(Array.Empty<CalendarException>());
        calendar.Setup(x => x.GetMonthTransferDaysAsync(period, default)).ReturnsAsync(Array.Empty<CalendarException>());
        await new GenerateScheduleCommandHandler(rules.Object, schedules.Object, calendar.Object, dbContext.Object)
            .Handle(new GenerateScheduleCommand("d", 2025, 2) { UserId = "u" }, default);
        schedule.WorkShifts.Should().HaveCount(20);
        schedule.WorkShifts.Sum(x => (x.EndsAtUtc - x.StartsAtUtc).TotalHours).Should().Be(160);
        dbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task MissingRulesDoNotWrite()
    {
        var dbContext = TestSupport.CreateDbContextMock();
        var handler = new GenerateScheduleCommandHandler(new Mock<IScheduleGenerationRuleRepository>().Object,
            new Mock<IScheduleRepository>().Object, new Mock<ICalendarRepository>().Object, dbContext.Object);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(new GenerateScheduleCommand("d", 2025, 2) { UserId = "u" }, default));
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ManualRulesDoNotOverwriteExistingShifts()
    {
        var rules = new Mock<IScheduleGenerationRuleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        rules.Setup(x => x.GetAsync("user-id", "department-id", TestSupport.Period, default))
            .ReturnsAsync(TestSupport.CreateRule(pattern: ShiftPattern.Manual));
        var handler = new GenerateScheduleCommandHandler(rules.Object, new Mock<IScheduleRepository>().Object,
            new Mock<ICalendarRepository>().Object, dbContext.Object);
        await Assert.ThrowsAsync<ScheduleService.Application.Extensions.Validation.ValidationException>(() =>
            handler.Handle(new GenerateScheduleCommand("department-id", 2026, 10) { UserId = "user-id" }, default));
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
