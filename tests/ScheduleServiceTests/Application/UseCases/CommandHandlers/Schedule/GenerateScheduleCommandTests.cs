namespace Application.UseCases.CommandHandlers.Schedule
{
    using MongoDB.Bson;
    using Moq;
    using ScheduleService.Application.UseCases.CommandHandlers.Schedule;
    using ScheduleService.Application.UseCases.Commands.Schedule;
    using ScheduleService.DataAccess.Repository;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;
    using Xunit;

    public class GenerateScheduleCommandTests
    {
        private readonly Mock<IUserRuleRepository> userRuleRepositoryMock = new Mock<IUserRuleRepository>();
        private readonly Mock<IScheduleRepository> scheduleRepositoryMock = new Mock<IScheduleRepository>();
        private readonly Mock<ICalendarRepository> calendarRepositoryMock = new Mock<ICalendarRepository>();

        [Fact]
        public async Task Handle_ReplacesSelectedUsersMonthSchedule()
        {
            var userId = ObjectId.GenerateNewId().ToString();
            var departmentId = ObjectId.GenerateNewId().ToString();
            var scheduleId = ObjectId.GenerateNewId().ToString();
            var command = new GenerateScheduleCommand(departmentId, 2025, 2)
            {
                UserId = userId,
            };
            var rules = new UserScheduleRules
            {
                UserId = userId,
                DepartmentId = departmentId,
                ScheduleId = scheduleId,
                OnlyFirstShift = true,
                HoursPerMonth = 160,
                MaxHoursPerDay = 8,
            };

            userRuleRepositoryMock
                .Setup(x => x.GetMonthScheduleRules(userId, departmentId, "february", 2025))
                .ReturnsAsync(rules);
            calendarRepositoryMock
                .Setup(x => x.GetMonthHolidays(2025, 2))
                .ReturnsAsync(new List<ScheduleService.Domain.Models.Calendar>());
            calendarRepositoryMock
                .Setup(x => x.GetMonthTransferDays(2025, 2))
                .ReturnsAsync(new List<ScheduleService.Domain.Models.Calendar>());

            var handler = CreateHandler();

            await handler.Handle(command, CancellationToken.None);

            scheduleRepositoryMock.Verify(x => x.ClearMonthSchedule(scheduleId), Times.Once);
            scheduleRepositoryMock.Verify(
                x => x.AddWorkDayAsync(scheduleId, It.IsAny<WorkDay>()),
                Times.Exactly(20));
        }

        [Fact]
        public async Task Handle_ThrowsWhenSelectedUsersRulesDoNotExist()
        {
            var userId = ObjectId.GenerateNewId().ToString();
            var departmentId = ObjectId.GenerateNewId().ToString();
            var command = new GenerateScheduleCommand(departmentId, 2025, 2)
            {
                UserId = userId,
            };

            userRuleRepositoryMock
                .Setup(x => x.GetMonthScheduleRules(userId, departmentId, "february", 2025))
                .ReturnsAsync((UserScheduleRules)null!);
            calendarRepositoryMock
                .Setup(x => x.GetMonthHolidays(2025, 2))
                .ReturnsAsync(new List<ScheduleService.Domain.Models.Calendar>());
            calendarRepositoryMock
                .Setup(x => x.GetMonthTransferDays(2025, 2))
                .ReturnsAsync(new List<ScheduleService.Domain.Models.Calendar>());

            var handler = CreateHandler();

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Equal("Schedule rules for this user not found", exception.Message);
            scheduleRepositoryMock.Verify(
                x => x.ClearMonthSchedule(It.IsAny<string>()),
                Times.Never);
        }

        private GenerateScheduleCommandHandler CreateHandler()
        {
            return new GenerateScheduleCommandHandler(
                userRuleRepositoryMock.Object,
                scheduleRepositoryMock.Object,
                calendarRepositoryMock.Object);
        }
    }
}
