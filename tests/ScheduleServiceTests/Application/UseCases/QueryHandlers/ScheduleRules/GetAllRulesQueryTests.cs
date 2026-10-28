namespace Application.UseCases.QueryHandlers.ScheduleRules;

using ScheduleService.Application.UseCases.Queries.ScheduleRules;
using ScheduleService.Application.UseCases.QueryHandlers.ScheduleRules;

public sealed class GetAllRulesQueryTests
{
    [Fact]
    public async Task Handle_ReturnsCurrentRulesWithScheduleIdentifiers()
    {
        var rules = new Mock<IScheduleGenerationRuleRepository>();
        var schedules = new Mock<IScheduleRepository>();
        var now = DateTime.UtcNow;
        var period = new DateOnly(now.Year, now.Month, 1);
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
        var oldRule = new ScheduleGenerationRule(
            "old-rule-id",
            "old-user-id",
            "department-id",
            period.AddMonths(-1),
            600,
            300,
            new TimeOnly(9, 0),
            new TimeOnly(15, 0),
            ShiftPattern.SecondShiftOnly);
        var schedule = new MonthlySchedule(
            "schedule-id",
            "user-id",
            "department-id",
            period,
            DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc));
        rules.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([oldRule, rule]);
        schedules.Setup(x => x.GetAsync("user-id", "department-id", period, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        var handler = new GetAllRulesQueryHandler(rules.Object, schedules.Object);

        var result = (await handler.Handle(new GetAllRulesQuery(), CancellationToken.None)).ToList();

        result.Should().HaveCount(2);
        result.Select(item => item.Id).Should().ContainInOrder("old-rule-id", "rule-id");
        result[0].ScheduleId.Should().BeEmpty();
        result[1].ScheduleId.Should().Be(schedule.Id);
        result[1].HoursPerMonth.Should().Be(8);
    }
}
