using FluentAssertions;
using Moq;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;
using Xunit;

namespace Application.UseCases.CommandHandlers.ScheduleRules;

public class CreateScheduleRulesTests
{
    private readonly Mock<IUserRuleRepository> mockUserRuleRepository;
    private readonly Mock<IScheduleRepository> mockScheduleRepository;
    private readonly CreateScheduleRulesCommandHandler handler;

    public CreateScheduleRulesTests()
    {
        this.mockUserRuleRepository = new Mock<IUserRuleRepository>();
        this.mockScheduleRepository = new Mock<IScheduleRepository>();
        this.handler = new CreateScheduleRulesCommandHandler(mockUserRuleRepository.Object, mockScheduleRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateScheduleAndUserRule()
    {
        // Arrange
        var command = new CreateScheduleRulesCommand("user123", "dept456", 2, 2025);

        mockScheduleRepository.Setup(repo => repo.AddAsync(It.IsAny<ScheduleService.Domain.Models.Schedule>())).Returns(Task.CompletedTask);
        mockUserRuleRepository.Setup(repo => repo.AddAsync(It.IsAny<UserScheduleRules>())).Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var scheduleIdFromRules = result.ScheduleRules.ScheduleId;

        result.Schedule.MonthName.Should().Be("february");
        result.Schedule.Id.Should().Be(scheduleIdFromRules);
        result.Schedule.WorkDays.Should().BeEmpty();

        result.ScheduleRules.UserId.Should().Be("user123");
        result.ScheduleRules.DepartmentId.Should().Be("dept456");
        result.ScheduleRules.Year.Should().Be(2025);
        result.ScheduleRules.MonthName.Should().Be("february");
        result.ScheduleRules.StartWorkDayTime.Should().Be(new TimeOnly(8, 0, 0));

        mockScheduleRepository.Verify(repo => repo.AddAsync(It.Is<ScheduleService.Domain.Models.Schedule>(s =>
            s.MonthName == "february")), Times.Once);

        mockUserRuleRepository.Verify(repo => repo.AddAsync(It.Is<UserScheduleRules>(r =>
            r.UserId == command.UserId &&
            r.DepartmentId == command.DepartmentId &&
            r.Year == command.Year &&
            r.MonthName == "february")), Times.Once);
    }
}