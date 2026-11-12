namespace Application.UseCases.CommandHandlers.ScheduleRules;

using ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;

public sealed class AddingScheduleRulesForNextMonthCommandTests
{
    [Fact]
    public async Task Handle_RejectsMissingCurrentRulesWithoutCommit()
    {
        var rules = new Mock<IScheduleGenerationRuleRepository>();
        var schedules = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        rules.Setup(x => x.GetAllByMonthAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ScheduleGenerationRule>());
        var handler = new AddingScheduleRulesForNextMonthCommandHandler(
            rules.Object,
            schedules.Object,
            dbContext.Object);

        var action = () => handler.Handle(
            new AddingScheduleRulesForNextMonthCommand(),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>();
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReusesExistingNextMonthEntitiesWithoutCommit()
    {
        var rules = new Mock<IScheduleGenerationRuleRepository>();
        var schedules = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var now = DateTime.UtcNow;
        var currentPeriod = new DateOnly(now.Year, now.Month, 1);
        var nextPeriod = currentPeriod.AddMonths(1);
        var currentRule = Rule("current-rule", currentPeriod);
        var nextRule = Rule("next-rule", nextPeriod);
        var schedule = new MonthlySchedule(
            "schedule-id",
            "user-id",
            "department-id",
            nextPeriod,
            DateTime.SpecifyKind(now, DateTimeKind.Utc));
        rules.Setup(x => x.GetAllByMonthAsync(currentPeriod, It.IsAny<CancellationToken>()))
            .ReturnsAsync([currentRule]);
        rules.Setup(x => x.GetAsync("user-id", "department-id", nextPeriod, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nextRule);
        schedules.Setup(x => x.GetAsync("user-id", "department-id", nextPeriod, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        var handler = new AddingScheduleRulesForNextMonthCommandHandler(
            rules.Object,
            schedules.Object,
            dbContext.Object);

        await handler.Handle(new AddingScheduleRulesForNextMonthCommand(), CancellationToken.None);

        schedules.Verify(x => x.AddAsync(It.IsAny<MonthlySchedule>(), It.IsAny<CancellationToken>()), Times.Never);
        rules.Verify(x => x.AddAsync(It.IsAny<ScheduleGenerationRule>(), It.IsAny<CancellationToken>()), Times.Never);
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ClonesCurrentRulesAndCreatesEmptySchedules()
    {
        var rules = new Mock<IScheduleGenerationRuleRepository>();
        var schedules = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var now = DateTime.UtcNow;
        var currentPeriod = new DateOnly(now.Year, now.Month, 1);
        var currentRule = new ScheduleGenerationRule(
            "rule-id",
            "user-id",
            "department-id",
            currentPeriod,
            480,
            240,
            new TimeOnly(8, 0),
            new TimeOnly(14, 30),
            ShiftPattern.AlternateByWeekdayA);
        rules.Setup(x => x.GetAllByMonthAsync(currentPeriod, It.IsAny<CancellationToken>()))
            .ReturnsAsync([currentRule]);
        var handler = new AddingScheduleRulesForNextMonthCommandHandler(
            rules.Object,
            schedules.Object,
            dbContext.Object);

        await handler.Handle(new AddingScheduleRulesForNextMonthCommand(), CancellationToken.None);

        var nextPeriod = currentPeriod.AddMonths(1);
        schedules.Verify(x => x.AddAsync(
            It.Is<MonthlySchedule>(item => item.Period == nextPeriod && item.UserId == "user-id"),
            CancellationToken.None), Times.Once);
        rules.Verify(x => x.AddAsync(
            It.Is<ScheduleGenerationRule>(item => item.Period == nextPeriod && item.Pattern == currentRule.Pattern),
            CancellationToken.None), Times.Once);
        dbContext.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    private static ScheduleGenerationRule Rule(string id, DateOnly period) =>
        new(
            id,
            "user-id",
            "department-id",
            period,
            480,
            240,
            new TimeOnly(8, 0),
            new TimeOnly(14, 30),
            ShiftPattern.AlternateByWeekdayA);
}
