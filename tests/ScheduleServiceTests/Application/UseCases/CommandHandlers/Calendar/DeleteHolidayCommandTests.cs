namespace Application.UseCases.CommandHandlers.Calendar;

using ScheduleService.Application.UseCases.CommandHandlers.Calendar;
using ScheduleService.Application.UseCases.Commands.Calendar;

public sealed class DeleteHolidayCommandTests
{
    [Fact]
    public async Task Handle_RejectsMissingHolidayWithoutCommit()
    {
        var repository = new Mock<ICalendarRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        repository.Setup(x => x.GetMonthHolidaysAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CalendarException>());
        var handler = new DeleteHolidayCommandHandler(repository.Object, dbContext.Object);

        var action = () => handler.Handle(
            new DeleteHolidayCommand(new DateOnly(2026, 10, 5)),
            CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
        repository.Verify(x => x.Remove(It.IsAny<CalendarException>()), Times.Never);
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RemovesHolidayAndCommits()
    {
        var repository = new Mock<ICalendarRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var holiday = new CalendarException("holiday-id", new DateOnly(2026, 10, 5));
        repository.Setup(x => x.GetMonthHolidaysAsync(new DateOnly(2026, 10, 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync([holiday]);
        var handler = new DeleteHolidayCommandHandler(repository.Object, dbContext.Object);

        await handler.Handle(new DeleteHolidayCommand(holiday.HolidayDate), CancellationToken.None);

        repository.Verify(x => x.Remove(holiday), Times.Once);
        dbContext.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}
