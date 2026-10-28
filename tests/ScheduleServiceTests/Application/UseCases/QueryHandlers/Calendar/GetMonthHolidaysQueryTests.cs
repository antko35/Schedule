namespace Application.UseCases.QueryHandlers.Calendar;

using ScheduleService.Application.UseCases.Queries.Calendar;
using ScheduleService.Application.UseCases.QueryHandlers.Calendar;

public sealed class GetMonthHolidaysQueryTests
{
    [Fact]
    public async Task Handle_MapsMonthlyHolidays()
    {
        var repository = new Mock<ICalendarRepository>();
        var holiday = new CalendarException("holiday-id", new DateOnly(2026, 10, 5));
        repository.Setup(x => x.GetMonthHolidaysAsync(new DateOnly(2026, 10, 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync([holiday]);
        var handler = new GetMonthHolidaysQueryHandler(repository.Object);

        var result = await handler.Handle(new GetMonthHolidaysQuery(2026, 10), CancellationToken.None);

        result.Should().ContainSingle().Which.Id.Should().Be("holiday-id");
        result[0].HolidayDate.Should().Be(holiday.HolidayDate);
    }
}
