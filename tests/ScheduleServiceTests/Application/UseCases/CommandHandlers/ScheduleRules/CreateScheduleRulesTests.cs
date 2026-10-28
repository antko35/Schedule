namespace Application.UseCases.CommandHandlers.ScheduleRules;

using ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;

public sealed class CreateScheduleRulesTests
{
    [Fact]
    public async Task Handle_ReusesExistingAggregateWithoutCommit()
    {
        var rules = new Mock<IScheduleGenerationRuleRepository>();
        var schedules = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var now = DateTime.UtcNow;
        var period = new DateOnly(now.Year, now.Month, 1);
        var schedule = new MonthlySchedule(
            "schedule-id",
            "user-id",
            "department-id",
            period,
            DateTime.SpecifyKind(now, DateTimeKind.Utc));
        var rule = new ScheduleGenerationRule(
            "rule-id",
            "user-id",
            "department-id",
            period,
            480,
            480,
            new TimeOnly(8, 0),
            new TimeOnly(14, 30),
            ShiftPattern.FirstShiftOnly);
        schedules.Setup(x => x.GetAsync("user-id", "department-id", period, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        rules.Setup(x => x.GetAsync("user-id", "department-id", period, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);
        var handler = new CreateScheduleRulesCommandHandler(rules.Object, schedules.Object, dbContext.Object);

        var result = await handler.Handle(
            new CreateScheduleRulesCommand("user-id", "department-id"),
            CancellationToken.None);

        result.Schedule.Id.Should().Be(schedule.Id);
        result.ScheduleRules.Id.Should().Be(rule.Id);
        schedules.Verify(x => x.AddAsync(It.IsAny<MonthlySchedule>(), It.IsAny<CancellationToken>()), Times.Never);
        rules.Verify(x => x.AddAsync(It.IsAny<ScheduleGenerationRule>(), It.IsAny<CancellationToken>()), Times.Never);
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CreatesScheduleAndDefaultRuleInOneCommit()
    {
        var rules = new Mock<IScheduleGenerationRuleRepository>();
        var schedules = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var handler = new CreateScheduleRulesCommandHandler(rules.Object, schedules.Object, dbContext.Object);

        var result = await handler.Handle(
            new CreateScheduleRulesCommand("user-id", "department-id"),
            CancellationToken.None);

        result.Schedule.Id.Should().NotBeNullOrWhiteSpace();
        result.ScheduleRules.ScheduleId.Should().Be(result.Schedule.Id);
        result.ScheduleRules.HoursPerMonth.Should().Be(132);
        schedules.Verify(x => x.AddAsync(
            It.Is<MonthlySchedule>(item => item.UserId == "user-id" && item.DepartmentId == "department-id"),
            CancellationToken.None), Times.Once);
        rules.Verify(x => x.AddAsync(
            It.Is<ScheduleGenerationRule>(item => item.TargetMinutes == 132 * 60),
            CancellationToken.None), Times.Once);
        dbContext.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}
