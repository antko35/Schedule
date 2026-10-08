using FluentAssertions;
using Moq;
using ScheduleService.Application.UseCases.CommandHandlers.Calendar;
using ScheduleService.Application.UseCases.Commands.Calendar;
using ScheduleService.Domain.Abstractions;
using Xunit;

namespace Application.UseCases.CommandHandlers.Calendar;

public class DeleteHolidaysCommandTests
{
    private readonly Mock<ICalendarRepository> calendarRepositoryMock = new();

    [Fact]
    public async Task Handle_ShouldDeleteAllSelectedHolidays()
    {
        // Arrange
        var firstDate = new DateOnly(2026, 1, 1);
        var secondDate = new DateOnly(2026, 5, 9);
        var holidays = new[]
        {
            new ScheduleService.Domain.Models.Calendar { HolidayDate = firstDate },
            new ScheduleService.Domain.Models.Calendar { HolidayDate = secondDate },
        };
        calendarRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(holidays);
        calendarRepositoryMock
            .Setup(x => x.RemoveHolidays(It.IsAny<IReadOnlyCollection<DateOnly>>()))
            .ReturnsAsync(2);
        var handler = new DeleteHolidaysCommandHandler(calendarRepositoryMock.Object);

        // Act
        await handler.Handle(
            new DeleteHolidaysCommand(new[] { firstDate, secondDate }),
            CancellationToken.None);

        // Assert
        calendarRepositoryMock.Verify(
            x => x.RemoveHolidays(
                It.Is<IReadOnlyCollection<DateOnly>>(dates =>
                    dates.Count == 2 &&
                    dates.Contains(firstDate) &&
                    dates.Contains(secondDate))),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotDeleteAnything_WhenAnyHolidayDoesNotExist()
    {
        // Arrange
        var existingDate = new DateOnly(2026, 1, 1);
        var missingDate = new DateOnly(2026, 5, 9);
        calendarRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(
                new[]
                {
                    new ScheduleService.Domain.Models.Calendar { HolidayDate = existingDate },
                });
        var handler = new DeleteHolidaysCommandHandler(calendarRepositoryMock.Object);

        // Act
        var act = () => handler.Handle(
            new DeleteHolidaysCommand(new[] { existingDate, missingDate }),
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{missingDate}*");
        calendarRepositoryMock.Verify(
            x => x.RemoveHolidays(It.IsAny<IReadOnlyCollection<DateOnly>>()),
            Times.Never);
    }
}
