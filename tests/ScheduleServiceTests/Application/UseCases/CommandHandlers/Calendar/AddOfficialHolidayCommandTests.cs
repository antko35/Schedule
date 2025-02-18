using AutoMapper;
using FluentAssertions;
using Moq;
using ScheduleService.Application.UseCases.CommandHandlers.Calendar;
using ScheduleService.Application.UseCases.Commands.Calendar;
using ScheduleService.Domain.Abstractions;
using Xunit;

namespace Application.UseCases.CommandHandlers.Calendar;

public class AddOfficialHolidayCommandTests
    {
        private readonly Mock<ICalendarRepository> calendarRepositoryMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly AddOfficialHolidayCommandHandler handler;

        public AddOfficialHolidayCommandTests()
        {
            this.calendarRepositoryMock = new Mock<ICalendarRepository>();
            this.mapperMock = new Mock<IMapper>();
            this.handler = new AddOfficialHolidayCommandHandler(calendarRepositoryMock.Object, mapperMock.Object);
        }

        [Fact]
        public async Task Handle_Should_AddHoliday_When_HolidayDoesNotExist()
        {
            // Arrange
            var holidayDate = new DateOnly(2025, 1, 1);
            var transerDate = new DateOnly();
            var command = new AddOfficialHolidayCommand(holidayDate, transerDate);

            var calendar = new ScheduleService.Domain.Models.Calendar
            {
                HolidayDate = holidayDate,
                TransferDate = null,
            };

            mapperMock.Setup(m => m.Map<ScheduleService.Domain.Models.Calendar>(command)).Returns(calendar);
            calendarRepositoryMock.Setup(r => r.GetMonthHolidays(holidayDate.Year, holidayDate.Month))
                .ReturnsAsync(new List<ScheduleService.Domain.Models.Calendar>());

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.HolidayDate.Should().Be(holidayDate);

            calendarRepositoryMock.Verify(r => r.AddAsync(calendar), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ThrowException_When_HolidayAlreadyExists()
        {
            // Arrange
            var holidayDate = new DateOnly(2023, 12, 25);
            var transerDate = new DateOnly();
            var command = new AddOfficialHolidayCommand(holidayDate, transerDate);

            var existingHoliday = new ScheduleService.Domain.Models.Calendar { HolidayDate = holidayDate };

            calendarRepositoryMock.Setup(r => r.GetMonthHolidays(holidayDate.Year, holidayDate.Month))
                .ReturnsAsync(new List<ScheduleService.Domain.Models.Calendar> { existingHoliday });

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Holiday {holidayDate} already exist");

            calendarRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ScheduleService.Domain.Models.Calendar>()), Times.Never);
        }
    }