namespace Application.UseCases.CommandHandlers.ScheduleRules;
using ScheduleService.Application.Services;
using ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
public sealed class SaveGenerationRulesCommandTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SavesNewOrExistingMonthAndReturnsCompatibleDto(bool existing)
    {
        var rules = new Mock<IScheduleGenerationRuleRepository>();
        var schedules = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var rule = TestSupport.CreateRule();
        var schedule = TestSupport.CreateSchedule();
        if (existing)
        {
            rules.Setup(x => x.GetAsync("user-id", "department-id", TestSupport.Period, default)).ReturnsAsync(rule);
            schedules.Setup(x => x.GetAsync("user-id", "department-id", TestSupport.Period, default)).ReturnsAsync(schedule);
        }
        var handler = new SaveGenerationRulesCommandHandler(new ScheduleMonthContextService(rules.Object, schedules.Object), dbContext.Object);
        var result = await handler.Handle(new SaveGenerationRulesCommand(160, 8, new TimeOnly(0, 0), true, false, false, false, false, false)
            { UserId = "user-id", DepartmentId = "department-id", Year = 2026, Month = 10 }, default);
        result.HoursPerMonth.Should().Be(160);
        result.StartWorkDayTime.Should().Be(new TimeOnly(0, 0));
        result.OnlyFirstShift.Should().BeTrue();
        result.ScheduleId.Should().NotBeNullOrEmpty();
        dbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        schedules.Verify(x => x.AddAsync(It.IsAny<MonthlySchedule>(), default), existing ? Times.Never() : Times.Once());
        rules.Verify(x => x.AddAsync(It.IsAny<ScheduleGenerationRule>(), default), existing ? Times.Never() : Times.Once());
    }

    [Fact]
    public async Task FailedRuleCreationNeverCommitsPartialSchedule()
    {
        var rules = new Mock<IScheduleGenerationRuleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        rules.Setup(x => x.AddAsync(It.IsAny<ScheduleGenerationRule>(), default)).ThrowsAsync(new InvalidOperationException("write failed"));
        var handler = new SaveGenerationRulesCommandHandler(new ScheduleMonthContextService(rules.Object, new Mock<IScheduleRepository>().Object), dbContext.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new SaveGenerationRulesCommand(160, 8, new TimeOnly(8, 0), true, false, false, false, false, false)
            { UserId = "u", DepartmentId = "d", Year = 2026, Month = 10 }, default));
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
