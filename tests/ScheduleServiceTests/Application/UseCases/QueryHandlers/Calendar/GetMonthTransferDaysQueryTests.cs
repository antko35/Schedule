using FluentAssertions;
using Moq;
using ScheduleService.Application.UseCases.Queries.Calendar;
using ScheduleService.Application.UseCases.QueryHandlers.Calendar;
using ScheduleService.Domain.Abstractions;
using Xunit;

namespace Application.UseCases.QueryHandlers.Calendar;

public class GetMonthTransferDaysQueryTests
{
    private readonly Mock<ICalendarRepository> calendarRepositoryMock;
    private readonly GetMonthTransferDaysQueryHandler handler;

    public GetMonthTransferDaysQueryTests()
    {
        calendarRepositoryMock = new Mock<ICalendarRepository>();
        handler = new GetMonthTransferDaysQueryHandler(calendarRepositoryMock.Object);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnEmptyList()
    {
        // Arrange
        var holidays = new List<ScheduleService.Domain.Models.Calendar> { };

        var query = new GetMonthTransferDaysQuery(2025, 1);

        calendarRepositoryMock.Setup(repo => repo.GetMonthHolidays(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((List<ScheduleService.Domain.Models.Calendar>?)null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task Handle_ShouldReturnTransferDays()
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

        var query = new GetMonthTransferDaysQuery(2025, 1);

        calendarRepositoryMock.Setup(repo => repo.GetMonthTransferDays(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(holidays);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(holidays);
    }
}