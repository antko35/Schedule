using FluentAssertions;
using MongoDB.Bson;
using Moq;
using ScheduleService.Application.UseCases.Queries.Schedule;
using ScheduleService.Application.UseCases.QueryHandlers.Schedule;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;
using Xunit;

namespace Application.UseCases.QueryHandlers.Schedule;

public class GetUserMonthScheduleQueryTests
{
    private readonly Mock<IScheduleRepository> scheduleRepositoryMock;
    private readonly Mock<IUserRuleRepository> userRuleRepositoryMock;
    private readonly GetUserMonthScheduleQueryHandler handler;

    public GetUserMonthScheduleQueryTests()
    {
        scheduleRepositoryMock = new Mock<IScheduleRepository>();
        userRuleRepositoryMock = new Mock<IUserRuleRepository>();
        handler = new GetUserMonthScheduleQueryHandler(
            scheduleRepositoryMock.Object,
            userRuleRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_ReturnSchedule_RulesAndScheduleExist()
    {
        // Arrange
        var userId = ObjectId.GenerateNewId().ToString();
        var departmentId = ObjectId.GenerateNewId().ToString();
        var year = 2025;
        var month = 12;
        var query = new GetUserMonthScheduleQuery ( userId, departmentId, year, month);

        var userRules = new UserScheduleRules
        {
            ScheduleId = ObjectId.GenerateNewId().ToString(),
            Year = 2025,
        };
        var schedule = new ScheduleService.Domain.Models.Schedule { Id = userRules.ScheduleId };

        userRuleRepositoryMock.Setup(r => r.GetMonthScheduleRules(userId, departmentId, "december", year))
            .ReturnsAsync(userRules);

        scheduleRepositoryMock.Setup(r => r.GetByIdAsync(userRules.ScheduleId))
            .ReturnsAsync(schedule);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(schedule);
        userRuleRepositoryMock.Verify(r => r.GetMonthScheduleRules(userId, departmentId, "december", year), Times.Once);
        scheduleRepositoryMock.Verify(r => r.GetByIdAsync(userRules.ScheduleId), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowException_When_UserRulesNotFound()
    {
        // Arrange
        var userId = ObjectId.GenerateNewId().ToString();
        var departmentId = ObjectId.GenerateNewId().ToString();
        var year = 2025;
        var month = 12;
        var query = new GetUserMonthScheduleQuery(userId,  departmentId, year, month);

        userRuleRepositoryMock.Setup(r => r.GetMonthScheduleRules(userId, departmentId, "december", year))
            .ReturnsAsync((UserScheduleRules)null);

        // Act
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Invalid input");

        userRuleRepositoryMock.Verify(r => r.GetMonthScheduleRules(userId, departmentId, "december", year), Times.Once);
        scheduleRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ThrowException_When_ScheduleNotFound()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        var departmentId = ObjectId.GenerateNewId().ToString();
        var year = 2025;
        var month = 12;
        var query = new GetUserMonthScheduleQuery(userId, departmentId, year, month);

        var userRules = new UserScheduleRules { ScheduleId = ObjectId.GenerateNewId().ToString() };

        userRuleRepositoryMock.Setup(r => r.GetMonthScheduleRules(userId, departmentId, "december", year))
            .ReturnsAsync(userRules);

        scheduleRepositoryMock.Setup(r => r.GetByIdAsync(userRules.ScheduleId))
            .ReturnsAsync((ScheduleService.Domain.Models.Schedule)null);

        // Act
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Error while getting schedule");

        userRuleRepositoryMock.Verify(r => r.GetMonthScheduleRules(userId, departmentId, "december", year), Times.Once);
        scheduleRepositoryMock.Verify(r => r.GetByIdAsync(userRules.ScheduleId), Times.Once);
    }
}