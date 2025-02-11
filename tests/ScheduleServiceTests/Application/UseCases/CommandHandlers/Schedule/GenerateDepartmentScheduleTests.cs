using MongoDB.Bson;
using Moq;
using ScheduleService.Application.UseCases.CommandHandlers.Schedule;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            command = new GenerateDepartmentScheduleCommand("id", 2025, 1);
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
                    DepartmentId = ObjectId.GenerateNewId().ToString(),
                    ScheduleId = ObjectId.GenerateNewId().ToString(),
                    EvenDOW = true,
                    FirstShift = true,
                    SecondShift = false,
                },
            };

            // var holidays = new List<Calendar>
            // {
            //     new Calendar
            //     {
            //         HolidayDate = new DateOnly(2025, 1, 1),
            //         TransferDate = null,
            //         MonthOfTransferDay = 1,
            //         MonthOfHoliday = 1,
            //         HolidayDayOfMonth = 1,
            //     },
            //     new Calendar
            //     {
            //         HolidayDate = new DateOnly(2025, 1, 6),
            //         TransferDate = new DateOnly(2025, 11, 1),
            //         MonthOfTransferDay = 1,
            //         MonthOfHoliday = 1,
            //         HolidayDayOfMonth = 1,
            //     },
            // };
            var holidays = new List<Calendar>();
            var transferDays = new List<Calendar>();

            // var transferDays = new List<Calendar>
            // {
            //     new Calendar
            //     {
            //          HolidayDate = new DateOnly(2025, 7, 1),
            //          TransferDate = new DateOnly(2025, 11, 1),
            //          MonthOfTransferDay = 1,
            //          MonthOfHoliday = 1,
            //          HolidayDayOfMonth = 1,
            //     },
            // };

            userRuleRepositoryMock
                .Setup(x => x.GetUsersRulesByDepartment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(usersRules);

            calendarRepositoryMock.Setup(x => x.GetMonthHolidays(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(holidays);

            calendarRepositoryMock.Setup(x => x.GetMonthTransferDays(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(transferDays);

            // Act
            await handler.Handle(command, CancellationToken.None);

            var a = 0;
            // Assert
        }

        [Fact]
        public async Task GenerateSchedule_UnevenDOW_FirstShift_WithoutHoliday()
        {
        }

        [Fact]
        public async Task GenerateSchedule_EvenDOM_FirstShift_WithoutHoliday()
        {
        }

        [Fact]
        public async Task GenerateSchedule_UnevenDOM_FirstShift_WithoutHoliday()
        {
        }

        [Fact]
        public async Task GenerateSchedule_EvenDOW_SecondShift_WithHoliday()
        {
        }

        [Fact]
        public async Task GenerateSchedule_EvenDOM_SecondShift_WithHoliday()
        {
        }
    }
}
