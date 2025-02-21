using FluentAssertions;
using Moq;
using ScheduleService.Application.UseCases.CommandHandlers.Schedule;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;
using Xunit;

namespace Application.UseCases.CommandHandlers.Schedule;

public class DeleteWorkDayCommandTests
{
    private readonly Mock<IScheduleRepository> mockScheduleRepository;
    private readonly Mock<IUserRuleRepository> mockUserRuleRepository;
    private readonly DeleteWorkDayCommandHandler handler;

    public DeleteWorkDayCommandTests()
    {
        this.mockScheduleRepository = new Mock<IScheduleRepository>();
        this.mockUserRuleRepository = new Mock<IUserRuleRepository>();
        this.handler = new DeleteWorkDayCommandHandler(
            mockUserRuleRepository.Object,
            mockScheduleRepository.Object);
    }

    [Fact]
    public async Task Handle_WhenScheduleExists_ShouldDeleteWorkDay()
    {
        // Arrange
        var command = new DeleteWorkDayCommand("user1", "dept1", new DateTime(2025, 1, 1));

        var scheduleId = "schedule1";
        var scheduleRules = new ScheduleService.Domain.Models.UserScheduleRules { ScheduleId = scheduleId };
        var workDay = new ScheduleService.Domain.Models.Schedule { };

        mockUserRuleRepository.Setup(repo => repo.GetMonthScheduleRules(command.UserId, command.DepartmentId, It.IsAny<string>(), 2025))
            .ReturnsAsync(scheduleRules);

        mockScheduleRepository.Setup(repo => repo.GetWorkDayAsync(scheduleId, command.WorkDay.Day))
            .ReturnsAsync(workDay);

        // Act
        var result = await handler.Handle(command, new CancellationToken());

        // Assert
        mockScheduleRepository.Verify(repo => repo.DeleteWorkDayAsync(scheduleId, command.WorkDay.Day), Times.Once);
        result.Should().BeEquivalentTo(workDay);
    }

    [Fact]
    public async Task Handle_WhenScheduleDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var command = new DeleteWorkDayCommand("user1", "dept1", new DateTime(2025, 1, 1));

        mockUserRuleRepository.Setup(repo => repo.GetMonthScheduleRules(command.UserId, command.DepartmentId, "january", 2025))
            .ReturnsAsync((UserScheduleRules)null);

        // Act
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Schedule for this user not found", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenWorkDayDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var command = new DeleteWorkDayCommand("user1", "dept1", new DateTime(2025, 1, 1));

        var scheduleId = "schedule1";
        var scheduleRules = new UserScheduleRules { ScheduleId = scheduleId };

        mockUserRuleRepository.Setup(repo => repo.GetMonthScheduleRules(command.UserId, command.DepartmentId, It.IsAny<string>(), command.WorkDay.Year))
            .ReturnsAsync(scheduleRules);

        mockScheduleRepository.Setup(repo => repo.GetWorkDayAsync(scheduleId, command.WorkDay.Day))
            .ReturnsAsync((ScheduleService.Domain.Models.Schedule)null);

        // Act
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("No work during day", exception.Message);
    }
}