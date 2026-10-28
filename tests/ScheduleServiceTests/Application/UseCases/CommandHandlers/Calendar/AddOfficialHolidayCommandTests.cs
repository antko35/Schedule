namespace Application.UseCases.CommandHandlers.Calendar;

using ScheduleService.Application.UseCases.CommandHandlers.Calendar;
using ScheduleService.Application.UseCases.Commands.Calendar;

public sealed class AddOfficialHolidayCommandTests
{
    [Fact]
    public async Task Handle_AddsHolidayAndCommits_WhenDateIsAvailable()
    {
        var repository = new Mock<ICalendarRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var holiday = new DateOnly(2026, 10, 5);
        repository.Setup(x => x.GetMonthHolidaysAsync(new DateOnly(2026, 10, 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CalendarException>());
        var handler = new AddOfficialHolidayCommandHandler(repository.Object, dbContext.Object);

        var result = await handler.Handle(
            new AddOfficialHolidayCommand(holiday, DateOnly.MinValue),
            CancellationToken.None);

        result.HolidayDate.Should().Be(holiday);
        result.TransferDate.Should().BeNull();
        repository.Verify(x => x.AddAsync(
            It.Is<CalendarException>(item => item.HolidayDate == holiday && item.TransferDate == null),
            CancellationToken.None), Times.Once);
        dbContext.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_RejectsDuplicateHolidayWithoutCommit()
    {
        var repository = new Mock<ICalendarRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var holiday = new DateOnly(2026, 10, 5);
        repository.Setup(x => x.GetMonthHolidaysAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new CalendarException("holiday-id", holiday)]);
        var handler = new AddOfficialHolidayCommandHandler(repository.Object, dbContext.Object);

        var action = () => handler.Handle(
            new AddOfficialHolidayCommand(holiday, DateOnly.MinValue),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>();
        repository.Verify(x => x.AddAsync(It.IsAny<CalendarException>(), It.IsAny<CancellationToken>()), Times.Never);
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
