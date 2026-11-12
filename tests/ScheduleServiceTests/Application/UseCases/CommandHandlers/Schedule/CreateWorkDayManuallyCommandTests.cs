namespace Application.UseCases.CommandHandlers.Schedule;

using Application;
using ScheduleService.Application.UseCases.CommandHandlers.Schedule;
using ScheduleService.Application.UseCases.Commands.Schedule;

public sealed class CreateWorkDayManuallyCommandTests
{
    [Fact]
    public async Task Handle_CreatesMissingMonthAndRulesTogether()
    {
        var schedules = new Mock<IScheduleRepository>();
        var rules = new Mock<IScheduleGenerationRuleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        MonthlySchedule? created = null;
        schedules.Setup(x => x.AddAsync(It.IsAny<MonthlySchedule>(), It.IsAny<CancellationToken>()))
            .Callback<MonthlySchedule, CancellationToken>((value, _) => created = value).Returns(Task.CompletedTask);
        var handler = new CreateWorkDayManuallyCommandHandler(
            new ScheduleService.Application.Services.ScheduleMonthContextService(rules.Object, schedules.Object), dbContext.Object);
        await handler.Handle(new CreateWorkDayManuallyCommand("u", "d", TestSupport.Utc(2026, 10, 7, 8), TestSupport.Utc(2026, 10, 7, 16)), default);
        created!.WorkShifts.Should().ContainSingle();
        rules.Verify(x => x.AddAsync(It.IsAny<ScheduleGenerationRule>(), It.IsAny<CancellationToken>()), Times.Once);
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PropagatesCancellationWithoutCommit()
    {
        var repository = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();
        repository.Setup(x => x.GetAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateOnly>(),
                cancellationSource.Token))
            .ThrowsAsync(new OperationCanceledException(cancellationSource.Token));
        var handler = new CreateWorkDayManuallyCommandHandler(new ScheduleService.Application.Services.ScheduleMonthContextService(new Mock<IScheduleGenerationRuleRepository>().Object, repository.Object), dbContext.Object);

        var action = () => handler.Handle(
            new CreateWorkDayManuallyCommand(
                "user-id",
                "department-id",
                TestSupport.Utc(2026, 10, 7, 8),
                TestSupport.Utc(2026, 10, 7, 16)),
            cancellationSource.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AddsShiftAndCommits()
    {
        var repository = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var schedule = TestSupport.CreateSchedule();
        repository.Setup(x => x.GetAsync("user-id", "department-id", TestSupport.Period, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        var handler = new CreateWorkDayManuallyCommandHandler(new ScheduleService.Application.Services.ScheduleMonthContextService(new Mock<IScheduleGenerationRuleRepository>().Object, repository.Object), dbContext.Object);
        var start = TestSupport.Utc(2026, 10, 7, 8);
        var end = TestSupport.Utc(2026, 10, 7, 16);

        var result = await handler.Handle(
            new CreateWorkDayManuallyCommand("user-id", "department-id", start, end),
            CancellationToken.None);

        result.Day.Should().Be(7);
        schedule.WorkShifts.Should().ContainSingle();
        schedule.WorkShifts.Single().StartsAtUtc.Should().Be(start);
        dbContext.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}
