namespace Application.UseCases.CommandHandlers.ScheduleRules;

using Application;
using ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;

public sealed class SetGenerationRulesTests
{
    [Fact]
    public async Task Handle_PreservesOmittedValuesAndAllowsMidnight()
    {
        var repository = new Mock<IScheduleGenerationRuleRepository>();
        var rule = TestSupport.CreateRule();
        var schedules = new Mock<IScheduleRepository>();
        schedules.Setup(x => x.GetAsync(rule.UserId, rule.DepartmentId, rule.Period, default)).ReturnsAsync(TestSupport.CreateSchedule());
        repository.Setup(x => x.GetByIdAsync(rule.Id, default)).ReturnsAsync(rule);
        var handler = new SetGenerationRulesCommandHandler(repository.Object, TestSupport.CreateDbContextMock().Object, schedules.Object);
        var result = await handler.Handle(new SetGenerationRulesCommand { ScheduleRulesId = rule.Id, StartWorkDayTime = new TimeOnly(0, 0) }, default);
        result.StartWorkDayTime.Should().Be(new TimeOnly(0, 0));
        result.OnlyFirstShift.Should().BeTrue();
        await handler.Handle(new SetGenerationRulesCommand { ScheduleRulesId = rule.Id, OnlyFirstShift = true }, default);
        rule.Pattern.Should().Be(ShiftPattern.FirstShiftOnly);
        result.ScheduleId.Should().Be("schedule-id");
        await handler.Handle(new SetGenerationRulesCommand { ScheduleRulesId = rule.Id, OnlyFirstShift = false }, default);
        rule.Pattern.Should().Be(ShiftPattern.Manual);
    }

    [Fact]
    public async Task Handle_SetsManualPattern_WhenEveryPatternFlagIsExplicitlyFalse()
    {
        var repository = new Mock<IScheduleGenerationRuleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var rule = TestSupport.CreateRule(pattern: ShiftPattern.FirstShiftOnly);
        repository.Setup(x => x.GetByIdAsync(rule.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);
        var handler = new SetGenerationRulesCommandHandler(repository.Object, dbContext.Object, new Mock<IScheduleRepository>().Object);

        await handler.Handle(
            new SetGenerationRulesCommand
            {
                ScheduleRulesId = rule.Id,
                OnlyFirstShift = false,
                OnlySecondShift = false,
                EvenDOW = false,
                UnEvenDOW = false,
                EvenDOM = false,
                UnEvenDOM = false,
            },
            CancellationToken.None);

        rule.Pattern.Should().Be(ShiftPattern.Manual);
    }

    [Fact]
    public async Task Handle_ReconfiguresRuleInMinutesAndCommits()
    {
        var repository = new Mock<IScheduleGenerationRuleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var rule = TestSupport.CreateRule();
        repository.Setup(x => x.GetByIdAsync(rule.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);
        var handler = new SetGenerationRulesCommandHandler(repository.Object, dbContext.Object, new Mock<IScheduleRepository>().Object);
        var command = new SetGenerationRulesCommand
        {
            ScheduleRulesId = rule.Id,
            HoursPerMonth = 10.5f,
            MaxHoursPerDay = 7.25f,
            StartWorkDayTime = new TimeOnly(9, 15),
            OnlySecondShift = true,
        };

        await handler.Handle(command, CancellationToken.None);

        rule.TargetMinutes.Should().Be(630);
        rule.MaxShiftMinutes.Should().Be(435);
        rule.FirstShiftStart.Should().Be(new TimeOnly(9, 15));
        rule.Pattern.Should().Be(ShiftPattern.SecondShiftOnly);
        dbContext.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}
