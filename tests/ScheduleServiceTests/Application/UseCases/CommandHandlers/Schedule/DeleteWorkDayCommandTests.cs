namespace Application.UseCases.CommandHandlers.Schedule;

using Application;
using ScheduleService.Application.UseCases.CommandHandlers.Schedule;
using ScheduleService.Application.UseCases.Commands.Schedule;

public sealed class DeleteWorkDayCommandTests
{
    [Fact]
    public async Task Handle_RejectsMissingWorkDayWithoutCommit()
    {
        var repository = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        repository.Setup(x => x.GetAsync("user-id", "department-id", TestSupport.Period, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestSupport.CreateSchedule());
        var handler = new DeleteWorkDayCommandHandler(repository.Object, dbContext.Object);

        var action = () => handler.Handle(
            new DeleteWorkDayCommand("user-id", "department-id", TestSupport.Utc(2026, 10, 7)),
            CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("No work during day");
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RejectsMissingScheduleWithoutCommit()
    {
        var repository = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        repository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MonthlySchedule?)null);
        var handler = new DeleteWorkDayCommandHandler(repository.Object, dbContext.Object);

        var action = () => handler.Handle(
            new DeleteWorkDayCommand("user-id", "department-id", TestSupport.Utc(2026, 10, 7)),
            CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RemovesShiftAndCommits()
    {
        var repository = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var schedule = TestSupport.CreateSchedule();
        schedule.SetShift(TestSupport.CreateShift(day: 7), TestSupport.Utc(2026, 9, 2));
        repository.Setup(x => x.GetAsync("user-id", "department-id", TestSupport.Period, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        var handler = new DeleteWorkDayCommandHandler(repository.Object, dbContext.Object);

        var result = await handler.Handle(
            new DeleteWorkDayCommand("user-id", "department-id", TestSupport.Utc(2026, 10, 7)),
            CancellationToken.None);

        schedule.WorkShifts.Should().BeEmpty();
        result.WorkDays.Should().BeEmpty();
        dbContext.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}
