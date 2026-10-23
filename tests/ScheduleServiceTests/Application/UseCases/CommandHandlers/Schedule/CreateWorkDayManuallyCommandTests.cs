using Moq;
using ScheduleService.Application.Services;
using ScheduleService.Application.UseCases.CommandHandlers.Schedule;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;
using Xunit;

namespace Application.UseCases.CommandHandlers.Schedule;

public class CreateWorkDayManuallyCommandTests
{
    private readonly Mock<IUserRuleRepository> userRuleRepositoryMock;
    private readonly Mock<IScheduleRepository> scheduleRepositoryMock;
    private readonly CreateWorkDayManuallyCommandHandler handler;

    public CreateWorkDayManuallyCommandTests()
    {
        this.userRuleRepositoryMock = new Mock<IUserRuleRepository>();
        this.scheduleRepositoryMock = new Mock<IScheduleRepository>();

        var monthContextService = new ScheduleMonthContextService(
            userRuleRepositoryMock.Object,
            scheduleRepositoryMock.Object);

        this.handler = new CreateWorkDayManuallyCommandHandler(
            scheduleRepositoryMock.Object,
            monthContextService);
    }

    [Fact]
    public async Task Handle_ShouldAddWorkDay_WhenNotExists()
    {
        // Arrange
        var command = new CreateWorkDayManuallyCommand
        (
            "userId",
            "DepartmentId",
            new DateTime(2025, 2, 10, 9, 0, 0),
            new DateTime(2025, 2, 10, 15, 30, 0)
        );

        var userRules = new UserScheduleRules { ScheduleId = "ScheduleId" };
        
        userRuleRepositoryMock
            .Setup(repo => repo.GetMonthScheduleRules(
                command.UserId, command.DepartmentId, It.IsAny<string>(), command.StartTime.Year))
            .ReturnsAsync(userRules);
        scheduleRepositoryMock
            .Setup(repo => repo.GetByIdAsync(userRules.ScheduleId))
            .ReturnsAsync(new ScheduleService.Domain.Models.Schedule { Id = userRules.ScheduleId });

        scheduleRepositoryMock
            .Setup(repo => repo.GetWorkDayAsync(userRules.ScheduleId, command.StartTime.Day))
            .ReturnsAsync((ScheduleService.Domain.Models.Schedule?)null);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        scheduleRepositoryMock.Verify(
            repo => repo.AddWorkDayAsync(userRules.ScheduleId, It.IsAny<WorkDay>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdate_WorkDayWhenExists()
    {
        // Arrange
        var command = new CreateWorkDayManuallyCommand
        (
            "userId",
            "DepartmentId",
            new DateTime(2025, 2, 10, 9, 0, 0),
            new DateTime(2025, 2, 10, 15, 30, 0)
        );

        var userRules = new UserScheduleRules { ScheduleId = "ScheduleId" };
        var existingWorkDay = new ScheduleService.Domain.Models.Schedule();
        existingWorkDay.WorkDays.Add(new WorkDay());

        userRuleRepositoryMock
            .Setup(repo => repo.GetMonthScheduleRules(
                command.UserId, command.DepartmentId, It.IsAny<string>(), command.StartTime.Year))
            .ReturnsAsync(userRules);
        scheduleRepositoryMock
            .Setup(repo => repo.GetByIdAsync(userRules.ScheduleId))
            .ReturnsAsync(new ScheduleService.Domain.Models.Schedule { Id = userRules.ScheduleId });

        scheduleRepositoryMock
            .Setup(repo => repo.GetWorkDayAsync(userRules.ScheduleId, command.StartTime.Day))
            .ReturnsAsync(existingWorkDay);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        scheduleRepositoryMock.Verify(
            repo => repo.UpdateWorkDayAsync(userRules.ScheduleId, It.IsAny<WorkDay>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateRulesAndSchedule_WhenMonthDoesNotExist()
    {
        var command = new CreateWorkDayManuallyCommand(
            "userId",
            "departmentId",
            new DateTime(2025, 2, 10, 9, 0, 0),
            new DateTime(2025, 2, 10, 15, 30, 0));
        string? createdScheduleId = null;

        userRuleRepositoryMock
            .Setup(repo => repo.GetMonthScheduleRules(
                command.UserId,
                command.DepartmentId,
                "february",
                command.StartTime.Year))
            .ReturnsAsync((UserScheduleRules?)null);
        scheduleRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<ScheduleService.Domain.Models.Schedule>()))
            .Callback<ScheduleService.Domain.Models.Schedule>(schedule => createdScheduleId = schedule.Id)
            .Returns(Task.CompletedTask);
        userRuleRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<UserScheduleRules>()))
            .Returns(Task.CompletedTask);
        scheduleRepositoryMock
            .Setup(repo => repo.GetWorkDayAsync(It.IsAny<string>(), command.StartTime.Day))
            .ReturnsAsync((ScheduleService.Domain.Models.Schedule?)null);

        await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(createdScheduleId);
        userRuleRepositoryMock.Verify(
            repo => repo.AddAsync(It.Is<UserScheduleRules>(rules =>
                rules.UserId == command.UserId
                && rules.DepartmentId == command.DepartmentId
                && rules.Year == command.StartTime.Year
                && rules.MonthName == "february"
                && rules.ScheduleId == createdScheduleId)),
            Times.Once);
        scheduleRepositoryMock.Verify(
            repo => repo.AddWorkDayAsync(createdScheduleId!, It.IsAny<WorkDay>()),
            Times.Once);
    }
}
