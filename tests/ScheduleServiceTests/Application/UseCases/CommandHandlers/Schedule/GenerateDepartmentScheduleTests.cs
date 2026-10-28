namespace Application.UseCases.CommandHandlers.Schedule;

using Application;
using ScheduleService.Application.UseCases.CommandHandlers.Schedule;
using ScheduleService.Application.UseCases.Commands.Schedule;

public sealed class GenerateDepartmentScheduleTests
{
    [Fact]
    public async Task Handle_RejectsManualRuleBeforeLoadingCalendarOrCommitting()
    {
        var rules = new Mock<IScheduleGenerationRuleRepository>();
        var schedules = new Mock<IScheduleRepository>();
        var calendar = new Mock<ICalendarRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var manualRule = TestSupport.CreateRule(pattern: ShiftPattern.Manual);
        rules.Setup(x => x.GetByDepartmentAsync("department-id", TestSupport.Period, It.IsAny<CancellationToken>()))
            .ReturnsAsync([manualRule]);
        var handler = new GenerateDepartmentScheduleCommandHandler(
            rules.Object,
            schedules.Object,
            calendar.Object,
            dbContext.Object);

        var action = () => handler.Handle(
            new GenerateDepartmentScheduleCommand("department-id", 2026, 10),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>();
        calendar.Verify(x => x.GetMonthHolidaysAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReplacesScheduleShiftsAndCommitsOnce()
    {
        var rules = new Mock<IScheduleGenerationRuleRepository>();
        var schedules = new Mock<IScheduleRepository>();
        var calendar = new Mock<ICalendarRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var rule = TestSupport.CreateRule(targetMinutes: 480, maxShiftMinutes: 480);
        var schedule = TestSupport.CreateSchedule();
        rules.Setup(x => x.GetByDepartmentAsync("department-id", TestSupport.Period, It.IsAny<CancellationToken>()))
            .ReturnsAsync([rule]);
        schedules.Setup(x => x.GetAsync("user-id", "department-id", TestSupport.Period, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        calendar.Setup(x => x.GetMonthHolidaysAsync(TestSupport.Period, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CalendarException>());
        calendar.Setup(x => x.GetMonthTransferDaysAsync(TestSupport.Period, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CalendarException>());
        var handler = new GenerateDepartmentScheduleCommandHandler(
            rules.Object,
            schedules.Object,
            calendar.Object,
            dbContext.Object);

        await handler.Handle(
            new GenerateDepartmentScheduleCommand("department-id", 2026, 10),
            CancellationToken.None);

        schedule.WorkShifts.Should().ContainSingle();
        schedule.WorkShifts.Single().MonthlyScheduleId.Should().Be(schedule.Id);
        dbContext.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}
