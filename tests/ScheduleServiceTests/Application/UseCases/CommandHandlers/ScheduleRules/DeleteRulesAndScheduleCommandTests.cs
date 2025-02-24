using FluentAssertions;
using Moq;
using ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;
using Xunit;

namespace Application.UseCases.CommandHandlers.ScheduleRules;

public class DeleteRulesAndScheduleCommandTests
{
    private readonly Mock<IScheduleRepository> scheduleRepositoryMock;
    private readonly Mock<IUserRuleRepository> userRuleRepositoryMock;
    private readonly DeleteRulesAndScheduleCommandHandler handler;

    public DeleteRulesAndScheduleCommandTests()
    {
        scheduleRepositoryMock = new Mock<IScheduleRepository>();
        userRuleRepositoryMock = new Mock<IUserRuleRepository>();
        handler = new DeleteRulesAndScheduleCommandHandler(
            scheduleRepositoryMock.Object,
            userRuleRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteAllSchedulesAndRules_WhenRulesExist()
    {
        // Arrange
        var userId = "user1";
        var departmentId = "dept1";
        var scheduleId1 = "schedule1";
        var scheduleId2 = "schedule2";

        var allUserRules = new List<UserScheduleRules>
        {
            new UserScheduleRules { ScheduleId = scheduleId1 },
            new UserScheduleRules { ScheduleId = scheduleId2 }
        };

        userRuleRepositoryMock
            .Setup(repo => repo.GetAllByIds(userId, departmentId))
            .ReturnsAsync(allUserRules);

        // Act
        await handler.Handle(new DeleteRulesAndScheduleCommand(userId,  departmentId), CancellationToken.None);

        // Assert
        scheduleRepositoryMock.Verify(repo => repo.DeleteScheduleAsync(scheduleId1), Times.Once);
        scheduleRepositoryMock.Verify(repo => repo.DeleteScheduleAsync(scheduleId2), Times.Once);
        userRuleRepositoryMock.Verify(repo => repo.DeleteAll(userId, departmentId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotDeleteAnything_WhenNoRulesExist()
    {
        // Arrange
        var userId = "user1";
        var departmentId = "dept1";

        userRuleRepositoryMock
            .Setup(repo => repo.GetAllByIds(userId, departmentId))
            .ReturnsAsync(new List<UserScheduleRules>());

        // Act
        await handler.Handle(new DeleteRulesAndScheduleCommand(userId, departmentId), CancellationToken.None);

        // Assert
        scheduleRepositoryMock.Verify(repo => repo.DeleteScheduleAsync(It.IsAny<string>()), Times.Never);
        userRuleRepositoryMock.Verify(repo => repo.DeleteAll(userId, departmentId), Times.Once);
    }
}