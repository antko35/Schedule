namespace Application.UseCases.QueryHandlers.Calendar;

using ScheduleService.Application.UseCases.Queries.Calendar;
using ScheduleService.Application.UseCases.QueryHandlers.Calendar;

public sealed class GetYearHolidaysQueryTests
{
    [Fact]
    public async Task Handle_MapsYearHolidays()
    {
        var repository = new Mock<ICalendarRepository>();
        var holiday = new CalendarException("holiday-id", new DateOnly(2026, 12, 25));
        repository.Setup(x => x.GetYearHolidaysAsync(2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync([holiday]);
        var handler = new GetYearHolidaysQueryHandler(repository.Object);

        var result = await handler.Handle(new GetYearHolidaysQuery(2026), CancellationToken.None);

        result.Should().ContainSingle().Which.Year.Should().Be(2026);
        result[0].HolidayDayOfMonth.Should().Be(25);
    }
}
