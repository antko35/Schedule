namespace Application.UseCases.QueryHandlers.ScheduleRules;

using Application;
using ScheduleService.Application.UseCases.Queries.ScheduleRules;
using ScheduleService.Application.UseCases.QueryHandlers.ScheduleRules;

public sealed class GetUserRulesQueryTests
{
    [Fact]
    public async Task Handle_RejectsMissingRuleWithoutReadingSchedule()
    {
        var rules = new Mock<IScheduleGenerationRuleRepository>();
        var schedules = new Mock<IScheduleRepository>();
        rules.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ScheduleGenerationRule?)null);
        var handler = new GetUserRulesQueryHandler(rules.Object, schedules.Object);

        var action = () => handler.Handle(
            new GetUserRulesQuery("user-id", "department-id", 10, 2026),
            CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Rules not found");
        schedules.Verify(x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateOnly>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsRuleWithRelatedScheduleId()
    {
        var rules = new Mock<IScheduleGenerationRuleRepository>();
        var schedules = new Mock<IScheduleRepository>();
        var rule = TestSupport.CreateRule();
        var schedule = TestSupport.CreateSchedule();
        rules.Setup(x => x.GetAsync("user-id", "department-id", TestSupport.Period, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);
        schedules.Setup(x => x.GetAsync("user-id", "department-id", TestSupport.Period, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        var handler = new GetUserRulesQueryHandler(rules.Object, schedules.Object);

        var result = await handler.Handle(
            new GetUserRulesQuery("user-id", "department-id", 10, 2026),
            CancellationToken.None);

        result.Id.Should().Be(rule.Id);
        result.ScheduleId.Should().Be(schedule.Id);
        result.OnlyFirstShift.Should().BeTrue();
    }
}
