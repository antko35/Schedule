namespace Application.UseCases.QueryHandlers.Calendar;
using ScheduleService.Application.UseCases.Queries.Calendar;
using ScheduleService.Application.UseCases.QueryHandlers.Calendar;
public sealed class GetHolidaysQueryTests
{
    [Fact]
    public async Task ReturnsCompatibleHolidayDtosOrderedByDate()
    {
        var later = new CalendarException("later", new DateOnly(2026, 5, 9), null);
        var earlier = new CalendarException("earlier", new DateOnly(2026, 1, 1), null);
        var repository = new Mock<ICalendarRepository>();
        repository.Setup(x => x.GetAllAsync(default)).ReturnsAsync(new[] { later, earlier });
        var result = await new GetHolidaysQueryHandler(repository.Object).Handle(new GetHolidaysQuery(), default);
        result.Select(x => x.Id).Should().Equal("earlier", "later");
        result[0].Year.Should().Be(2026);
    }
}
