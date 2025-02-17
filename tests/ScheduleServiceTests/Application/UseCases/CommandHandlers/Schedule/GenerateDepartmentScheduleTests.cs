using MongoDB.Bson;
using Moq;
using ScheduleService.Application.UseCases.CommandHandlers.Schedule;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Application.UseCases.CommandHandlers.Schedule
{
    public class GenerateDepartmentScheduleTests
    {
        private readonly Mock<IUserRuleRepository> userRuleRepositoryMock;
        private readonly Mock<IScheduleRepository> scheduleRepositoryMock;
        private readonly Mock<ICalendarRepository> calendarRepositoryMock;

        private readonly GenerateDepartmentScheduleCommandHandler handler;
        private readonly GenerateDepartmentScheduleCommand command;

        public GenerateDepartmentScheduleTests()
        {
            userRuleRepositoryMock = new Mock<IUserRuleRepository>();
            scheduleRepositoryMock = new Mock<IScheduleRepository>();
            calendarRepositoryMock = new Mock<ICalendarRepository>();

            handler = new GenerateDepartmentScheduleCommandHandler(
                userRuleRepositoryMock.Object,
                scheduleRepositoryMock.Object,
                calendarRepositoryMock.Object);

            command = new GenerateDepartmentScheduleCommand("id", 2025, 2);
        }

        [Fact]
        public async Task GenerateSchedule_EvenDOW_FirstShift_WithoutHoliday()
        {
            // Arrange
            var usersRules = new List<UserScheduleRules>
            {
                new UserScheduleRules
                {
                    UserId = ObjectId.GenerateNewId().ToString(),
                    DepartmentId = "department1",
                    ScheduleId = ObjectId.GenerateNewId().ToString(),
                    EvenDOW = true,
                },
            };

            var holidays = new List<Calendar>();
            var transferDays = new List<Calendar>();

            userRuleRepositoryMock
                .Setup(x => x.GetUsersRulesByDepartment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(usersRules);

            calendarRepositoryMock
                .Setup(x => x.GetMonthHolidays(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(holidays);

            calendarRepositoryMock
                .Setup(x => x.GetMonthTransferDays(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(transferDays);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            userRuleRepositoryMock.Verify(x =>
                x.GetUsersRulesByDepartment(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()), Times.Once);

            scheduleRepositoryMock.Verify(
                x => x.DeleteMonthSchedule(It.IsAny<string>()), Times.Once);

            scheduleRepositoryMock.Verify(
                x => x.AddWorkDayAsync(usersRules[0].ScheduleId, It.IsAny<WorkDay>()), Times.Exactly(20));
        }

        [Fact]
        public async Task GenerateSchedule_ShouldThrowException_WhenNoRulesFound()
        {
            // Arrange
            userRuleRepositoryMock
                .Setup(x => x.GetUsersRulesByDepartment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((new List<UserScheduleRules>()));

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(command, CancellationToken.None));

            // Assert
            Assert.Equal("Schedule rules for this department not found", exception.Message);
        }
    }
}
