using FluentAssertions;
using Moq;
using ScheduleService.Application.UseCases.Queries.Calendar;
using ScheduleService.Application.UseCases.QueryHandlers.Calendar;
using ScheduleService.Domain.Abstractions;
using Xunit;

namespace Application.UseCases.QueryHandlers.Calendar;

public class GetMonthHolidaysQueryTests
{
    private readonly Mock<ICalendarRepository> calendarRepositoryMock;
    private readonly GetMonthHolidaysQueryHandler handler;
    
    public GetMonthHolidaysQueryTests()
    {
        calendarRepositoryMock = new Mock<ICalendarRepository>();
        handler = new GetMonthHolidaysQueryHandler(calendarRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList()
    {
        // Arrange
        var holidays = new List<ScheduleService.Domain.Models.Calendar> { };

        var query = new GetMonthHolidaysQuery(2025, 1);

        calendarRepositoryMock.Setup(repo => repo.GetMonthHolidays(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(holidays);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(holidays);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnHolidays()
    {
        // Arrange
        var holidays = new List<ScheduleService.Domain.Models.Calendar>
        {
            new ScheduleService.Domain.Models.Calendar()
            {
                Id = "1",
            },
        };

        var query = new GetMonthHolidaysQuery(2025, 1);

        calendarRepositoryMock.Setup(repo => repo.GetMonthHolidays(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(holidays);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(holidays);
    }
}