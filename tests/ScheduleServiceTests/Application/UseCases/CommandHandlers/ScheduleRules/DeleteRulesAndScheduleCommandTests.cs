namespace Application.UseCases.CommandHandlers.ScheduleRules;

using Application;
using ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;

public sealed class DeleteRulesAndScheduleCommandTests
{
    [Fact]
    public async Task Handle_CommitsEmptyDeleteAsIdempotentOperation()
    {
        var rules = new Mock<IScheduleGenerationRuleRepository>();
        var schedules = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        rules.Setup(x => x.GetAllByUserDepartmentAsync("user-id", "department-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ScheduleGenerationRule>());
        var handler = new DeleteRulesAndScheduleCommandHandler(
            schedules.Object,
            rules.Object,
            dbContext.Object);

        await handler.Handle(
            new DeleteRulesAndScheduleCommand("user-id", "department-id"),
            CancellationToken.None);

        rules.Verify(x => x.RemoveRange(It.Is<IEnumerable<ScheduleGenerationRule>>(items => !items.Any())), Times.Once);
        dbContext.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_RemovesRulesAndTheirSchedulesInOneCommit()
    {
        var rules = new Mock<IScheduleGenerationRuleRepository>();
        var schedules = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var rule = TestSupport.CreateRule();
        var schedule = TestSupport.CreateSchedule();
        rules.Setup(x => x.GetAllByUserDepartmentAsync("user-id", "department-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync([rule]);
        schedules.Setup(x => x.GetAsync("user-id", "department-id", TestSupport.Period, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        var handler = new DeleteRulesAndScheduleCommandHandler(
            schedules.Object,
            rules.Object,
            dbContext.Object);

        await handler.Handle(
            new DeleteRulesAndScheduleCommand("user-id", "department-id"),
            CancellationToken.None);

        schedules.Verify(x => x.Remove(schedule), Times.Once);
        rules.Verify(x => x.RemoveRange(It.Is<IEnumerable<ScheduleGenerationRule>>(items => items.Single() == rule)), Times.Once);
        dbContext.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}
