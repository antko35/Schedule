using FluentAssertions;
using FluentAssertions.Primitives;
using MongoDB.Bson;
using Moq;
using ScheduleService.Application.UseCases.CommandHandlers.Calendar;
using ScheduleService.Application.UseCases.Commands.Calendar;
using ScheduleService.Domain.Abstractions;
using Xunit;

namespace Application.UseCases.CommandHandlers.Calendar;

public class DeleteHolidayCommandTests
{
    private readonly Mock<ICalendarRepository> calendarRepositoryMock;
    private readonly DeleteHolidayCommandHandler handler;

    public DeleteHolidayCommandTests()
    {
        calendarRepositoryMock = new Mock<ICalendarRepository>();
        handler = new DeleteHolidayCommandHandler(calendarRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_DeleteHoliday_When_HolidayExists()
    {
        // Arrange
        var holidayDate = new DateOnly(2025, 1, 1);
        var command = new DeleteHolidayCommand(holidayDate);

        var holidays = new List<ScheduleService.Domain.Models.Calendar>
        {
            new ScheduleService.Domain.Models.Calendar
            {
                Id = ObjectId.GenerateNewId().ToString(),
                HolidayDate = holidayDate,
            },
        };

        calendarRepositoryMock.Setup(r => r.GetMonthHolidays(holidayDate.Year, holidayDate.Month))
            .ReturnsAsync(holidays);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        calendarRepositoryMock.Verify(r => r.RemoveAsync(holidays[0].Id), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowKeyNotFoundException_When_HolidayDoesNotExist()
    {
        // Arrange
        var holidayDate = new DateOnly(2023, 12, 25);
        var command = new DeleteHolidayCommand(holidayDate);

        calendarRepositoryMock.Setup(r => r.GetMonthHolidays(holidayDate.Year, holidayDate.Month))
            .ReturnsAsync(new List<ScheduleService.Domain.Models.Calendar>());

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Day not found");

        calendarRepositoryMock.Verify(r => r.RemoveAsync(It.IsAny<string>()), Times.Never);
    }
}