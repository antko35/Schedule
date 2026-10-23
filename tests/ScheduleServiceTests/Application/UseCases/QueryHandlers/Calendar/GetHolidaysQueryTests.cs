using FluentAssertions;
using Moq;
using ScheduleService.Application.UseCases.Queries.Calendar;
using ScheduleService.Application.UseCases.QueryHandlers.Calendar;
using ScheduleService.Domain.Abstractions;
using Xunit;

namespace Application.UseCases.QueryHandlers.Calendar;

public class GetHolidaysQueryTests
{
    [Fact]
    public async Task Handle_ShouldReturnAllHolidaysOrderedByDate()
    {
        // Arrange
        var laterHoliday = new ScheduleService.Domain.Models.Calendar
        {
            HolidayDate = new DateOnly(2026, 5, 9),
        };
        var earlierHoliday = new ScheduleService.Domain.Models.Calendar
        {
            HolidayDate = new DateOnly(2026, 1, 1),
        };
        var repository = new Mock<ICalendarRepository>();
        repository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new[] { laterHoliday, earlierHoliday });
        var handler = new GetHolidaysQueryHandler(repository.Object);

        // Act
        var result = await handler.Handle(new GetHolidaysQuery(), CancellationToken.None);

        // Assert
        result.Should().ContainInOrder(earlierHoliday, laterHoliday);
    }
}
