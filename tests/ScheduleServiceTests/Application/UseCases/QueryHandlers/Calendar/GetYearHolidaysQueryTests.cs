using FluentAssertions;
using Moq;
using ScheduleService.Application.UseCases.Queries.Calendar;
using ScheduleService.Application.UseCases.QueryHandlers.Calendar;
using ScheduleService.Domain.Abstractions;
using Xunit;

namespace Application.UseCases.QueryHandlers.Calendar;

public class GetYearHolidaysQueryTests
{
    private readonly Mock<ICalendarRepository> calendarRepositoryMock;
    private readonly GetYearHolidaysQueryHandler handler;
    
    public GetYearHolidaysQueryTests()
    {
        calendarRepositoryMock = new Mock<ICalendarRepository>();
        handler = new GetYearHolidaysQueryHandler(calendarRepositoryMock.Object);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetYearHolidaysQuery(2025);

        calendarRepositoryMock.Setup(repo => repo.GetYearHolidays(It.IsAny<int>()))
            .ReturnsAsync((List<ScheduleService.Domain.Models.Calendar>?)null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
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
                HolidayDate = new DateOnly(2025, 1, 1),
                TransferDate = new DateOnly(2025, 1, 2),
            },
        };

        var query = new GetYearHolidaysQuery(2025);

        calendarRepositoryMock.Setup(repo => repo.GetYearHolidays(It.IsAny<int>()))
            .ReturnsAsync(holidays);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(holidays);
    }
}