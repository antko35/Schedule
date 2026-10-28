namespace Application.UseCases.CommandHandlers.Schedule;

using Application;
using ScheduleService.Application.UseCases.CommandHandlers.Schedule;
using ScheduleService.Application.UseCases.Commands.Schedule;

public sealed class DeleteUserMonthScheduleTests
{
    [Fact]
    public async Task Handle_ReturnsNothingToDeleteWithoutCommit_WhenScheduleIsEmpty()
    {
        var repository = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var schedule = TestSupport.CreateSchedule();
        repository.Setup(x => x.GetAsync("user-id", "department-id", TestSupport.Period, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        var handler = new DeleteUserMonthScheduleCommandHandler(repository.Object, dbContext.Object);

        var result = await handler.Handle(
            new DeleteUserMonthScheduleCommand("user-id", "department-id", 10, 2026),
            CancellationToken.None);

        result.Should().Be("Nothing to delete");
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RejectsMissingScheduleWithoutCommit()
    {
        var repository = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        repository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MonthlySchedule?)null);
        var handler = new DeleteUserMonthScheduleCommandHandler(repository.Object, dbContext.Object);

        var action = () => handler.Handle(
            new DeleteUserMonthScheduleCommand("user-id", "department-id", 10, 2026),
            CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ClearsAllShiftsAndCommits()
    {
        var repository = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var schedule = TestSupport.CreateSchedule();
        schedule.SetShift(TestSupport.CreateShift(), TestSupport.Utc(2026, 9, 2));
        repository.Setup(x => x.GetAsync("user-id", "department-id", TestSupport.Period, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        var handler = new DeleteUserMonthScheduleCommandHandler(repository.Object, dbContext.Object);

        var result = await handler.Handle(
            new DeleteUserMonthScheduleCommand("user-id", "department-id", 10, 2026),
            CancellationToken.None);

        result.Should().Contain("schedule-id");
        schedule.WorkShifts.Should().BeEmpty();
        dbContext.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}
