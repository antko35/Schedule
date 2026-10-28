namespace Application.UseCases.CommandHandlers.Calendar;
using ScheduleService.Application.UseCases.CommandHandlers.Calendar;
using ScheduleService.Application.UseCases.Commands.Calendar;
public sealed class DeleteHolidaysCommandTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ValidatesAllDatesBeforeDeletingAndCommitsOnce(bool missing)
    {
        var date = new DateOnly(2026, 1, 1);
        var holiday = new CalendarException("h", date, null);
        var repository = new Mock<ICalendarRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        repository.Setup(x => x.GetAllAsync(default)).ReturnsAsync(new[] { holiday });
        var handler = new DeleteHolidaysCommandHandler(repository.Object, dbContext.Object);
        var command = new DeleteHolidaysCommand(missing ? new[] { date, date.AddDays(1) } : new[] { date, date });
        if (missing)
            await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, default));
        else
            await handler.Handle(command, default);
        repository.Verify(x => x.Remove(It.IsAny<CalendarException>()), missing ? Times.Never() : Times.Once());
        dbContext.Verify(x => x.SaveChangesAsync(default), missing ? Times.Never() : Times.Once());
    }
}
