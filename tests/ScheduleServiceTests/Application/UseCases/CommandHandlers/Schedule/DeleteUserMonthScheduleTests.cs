using Moq;
using ScheduleService.Application.UseCases.CommandHandlers.Schedule;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;
using Xunit;

namespace Application.UseCases.CommandHandlers.Schedule;

public class DeleteUserMonthScheduleTests
{
        private readonly Mock<IScheduleRepository> mockScheduleRepository;
        private readonly Mock<IUserRuleRepository> mockUserRuleRepository;
        private readonly DeleteUserMonthScheduleCommandHandler handler;

        public DeleteUserMonthScheduleTests()
        {
            this.mockScheduleRepository = new Mock<IScheduleRepository>();
            this.mockUserRuleRepository = new Mock<IUserRuleRepository>();
            this.handler = new DeleteUserMonthScheduleCommandHandler(
                mockScheduleRepository.Object,
                mockUserRuleRepository.Object);
        }

        [Fact]
        public async Task Handle_ShouldDeleteSchedule_WhenScheduleExists()
        {
            // Arrange
            var request = new DeleteUserMonthScheduleCommand("user1", "dept1", 2, 2025);

            var userRules = new UserScheduleRules
            {
                ScheduleId = "schedule1"
            };

            mockUserRuleRepository
                .Setup(x => x.GetMonthScheduleRules(request.UserId, request.DepartmentId, "february", request.Year))
                .ReturnsAsync(userRules);

            mockScheduleRepository
                .Setup(x => x.DeleteMonthSchedule(userRules.ScheduleId))
                .ReturnsAsync(new MongoDB.Driver.UpdateResult.Acknowledged(1, 1, null));

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal($" Schedule {userRules.ScheduleId} was cleaned.", result);
            mockScheduleRepository.Verify(x => x.DeleteMonthSchedule(userRules.ScheduleId), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnNothingToDelete_WhenScheduleDoesNotExist()
        {
            // Arrange
            var request = new DeleteUserMonthScheduleCommand("user1", "dept1", 2, 2025);

            var userRules = new UserScheduleRules
            {
                ScheduleId = "schedule1"
            };

            mockUserRuleRepository
                .Setup(x => x.GetMonthScheduleRules(request.UserId, request.DepartmentId, "february", request.Year))
                .ReturnsAsync(userRules);

            mockScheduleRepository
                .Setup(x => x.DeleteMonthSchedule(userRules.ScheduleId))
                .ReturnsAsync(new MongoDB.Driver.UpdateResult.Acknowledged(0, 0, null));

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal("Nothing to delete", result);
            mockScheduleRepository.Verify(x => x.DeleteMonthSchedule(userRules.ScheduleId), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenUserRulesNotFound()
        {
            // Arrange
            var request = new DeleteUserMonthScheduleCommand("user1", "dept1", 2, 2025);

            mockUserRuleRepository
                .Setup(x => x.GetMonthScheduleRules(request.UserId, request.DepartmentId, "february", request.Year))
                .ReturnsAsync((UserScheduleRules)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(request, CancellationToken.None));

            Assert.Equal("Invalid input", exception.Message);
        }
}