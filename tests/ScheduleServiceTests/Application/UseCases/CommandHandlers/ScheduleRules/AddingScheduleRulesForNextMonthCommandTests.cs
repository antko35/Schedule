using FluentAssertions;
using MongoDB.Bson;
using Moq;
using ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;
using Xunit;

namespace Application.UseCases.CommandHandlers.ScheduleRules;

public class AddingScheduleRulesForNextMonthCommandTests
{
    private readonly Mock<IScheduleRepository> scheduleRepositoryMock;
    private readonly Mock<IUserRuleRepository> scheduleRuleRepositoryMock;
    private readonly AddingScheduleRulesForNextMonthCommandHandler handler;

    public AddingScheduleRulesForNextMonthCommandTests()
    {
        scheduleRepositoryMock = new Mock<IScheduleRepository>();
        scheduleRuleRepositoryMock = new Mock<IUserRuleRepository>();
        handler = new AddingScheduleRulesForNextMonthCommandHandler(
            scheduleRuleRepositoryMock.Object,
            scheduleRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldAddRulesAndSchedules_WhenRulesExist()
    {
        // Arrange
        var currentDate = DateTime.Now;
        var nextMonthDate = currentDate.AddMonths(1);
        var currentMonth = currentDate.ToString("MMMM").ToLower();
        var nextMonth = nextMonthDate.ToString("MMMM").ToLower();

        var allScheduleRules = new List<UserScheduleRules>
        {
            new UserScheduleRules
            {
                UserId = "user1",
                DepartmentId = "dept1",
                Year = currentDate.Year,
                MonthName = currentMonth,
                ScheduleId = ObjectId.GenerateNewId().ToString(),
            },
        };

        scheduleRuleRepositoryMock
            .Setup(repo => repo.GetAllRulesByMonth(currentMonth))
            .ReturnsAsync(allScheduleRules);

        // Act
        await handler.Handle(new AddingScheduleRulesForNextMonthCommand(), CancellationToken.None);

        // Assert
        scheduleRuleRepositoryMock.Verify(
            repo => repo.AddRangeAsync(It.IsAny<IEnumerable<UserScheduleRules>>()),
            Times.Once);

        scheduleRepositoryMock.Verify(
            repo => repo.AddRangeAsync(It.IsAny<IEnumerable<ScheduleService.Domain.Models.Schedule>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRulesAreEmpty()
    {
        // Arrange
        var currentDate = DateTime.Now;
        var currentMonth = currentDate.ToString("MMMM").ToLower();

        scheduleRuleRepositoryMock
            .Setup(repo => repo.GetAllRulesByMonth(currentMonth))
            .ReturnsAsync((List<UserScheduleRules>)null);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new AddingScheduleRulesForNextMonthCommand(), CancellationToken.None));

        // Assert
        scheduleRuleRepositoryMock.Verify(
            repo => repo.AddRangeAsync(It.IsAny<IEnumerable<UserScheduleRules>>()),
            Times.Never);

        scheduleRepositoryMock.Verify(
            repo => repo.AddRangeAsync(It.IsAny<IEnumerable<ScheduleService.Domain.Models.Schedule>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldAddCorrectData_WhenRulesExist()
    {
        // Arrange
        var currentDate = DateTime.Now;
        var nextMonthDate = currentDate.AddMonths(1);
        var currentMonth = currentDate.ToString("MMMM").ToLower();
        var nextMonth = nextMonthDate.ToString("MMMM").ToLower();

        var allScheduleRules = new List<UserScheduleRules>
        {
            new UserScheduleRules
            {
                UserId = "user1",
                DepartmentId = "dept1",
                Year = currentDate.Year,
                MonthName = currentMonth,
                ScheduleId = ObjectId.GenerateNewId().ToString(),
            },
        };

        scheduleRuleRepositoryMock
            .Setup(repo => repo.GetAllRulesByMonth(currentMonth))
            .ReturnsAsync(allScheduleRules);

        List<UserScheduleRules> addedRules = null;
        scheduleRuleRepositoryMock
            .Setup(repo => repo.AddRangeAsync(It.IsAny<IEnumerable<UserScheduleRules>>()))
            .Callback<IEnumerable<UserScheduleRules>>(rules => addedRules = rules.ToList());

        List<ScheduleService.Domain.Models.Schedule> addedSchedules = null;
        scheduleRepositoryMock
            .Setup(repo => repo.AddRangeAsync(It.IsAny<IEnumerable<ScheduleService.Domain.Models.Schedule>>()))
            .Callback<IEnumerable<ScheduleService.Domain.Models.Schedule>>(schedules => addedSchedules = schedules.ToList());

        // Act
        await handler.Handle(new AddingScheduleRulesForNextMonthCommand(), CancellationToken.None);

        // Assert
        addedRules.Should().HaveCount(1);
        addedRules[0].UserId.Should().Be("user1");
        addedRules[0].DepartmentId.Should().Be("dept1");
        addedRules[0].Year.Should().Be(nextMonthDate.Year);
        addedRules[0].MonthName.Should().Be(nextMonth);

        addedSchedules.Should().HaveCount(1);
        addedSchedules[0].MonthName.Should().Be(nextMonth);
    }
}