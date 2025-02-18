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

public class GetDepartmentMonthScheduleQueryTests
{
    private readonly Mock<IScheduleRepository> scheduleRepositoryMock;
    private readonly Mock<IUserRuleRepository> userRuleRepositoryMock;
    private readonly GetDepartmentMonthScheduleQueryHandler handler;

    public GetDepartmentMonthScheduleQueryTests()
    {
        scheduleRepositoryMock = new Mock<IScheduleRepository>();
        userRuleRepositoryMock = new Mock<IUserRuleRepository>();
        handler = new GetDepartmentMonthScheduleQueryHandler(
            scheduleRepositoryMock.Object,
            userRuleRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_ReturnSchedules_When_RulesExist()
    {
        // Arrange
        var departmentId = ObjectId.GenerateNewId().ToString();
        var year = 2023;
        var month = 12;
        var query = new GetDepartmentMonthScheduleQuery(departmentId, year, month);

        var usersRules = new List<UserScheduleRules>
        {
            new UserScheduleRules { ScheduleId = ObjectId.GenerateNewId().ToString() },
            new UserScheduleRules { ScheduleId = ObjectId.GenerateNewId().ToString()}
        };

        var schedules = new List<ScheduleService.Domain.Models.Schedule>
        {
            new ScheduleService.Domain.Models.Schedule { Id = usersRules[0].ScheduleId },
            new ScheduleService.Domain.Models.Schedule { Id = usersRules[1].ScheduleId }
        };

        userRuleRepositoryMock.Setup(r => r.GetUsersRulesByDepartment(departmentId, "december", year))
            .ReturnsAsync(usersRules);

        scheduleRepositoryMock.Setup(r => r.GetMonthSchedule(usersRules[0].ScheduleId))
            .ReturnsAsync(schedules[0]);

        scheduleRepositoryMock.Setup(r => r.GetMonthSchedule(usersRules[1].ScheduleId))
            .ReturnsAsync(schedules[1]);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(schedules);
        userRuleRepositoryMock.Verify(r => r.GetUsersRulesByDepartment(departmentId, "december", year), Times.Once);
        scheduleRepositoryMock.Verify(r => r.GetMonthSchedule(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_Should_ThrowInvalidOperationException_When_RulesNotFound()
    {
        // Arrange
        var departmentId = ObjectId.GenerateNewId().ToString();
        var year = 2023;
        var month = 12;
        var query = new GetDepartmentMonthScheduleQuery (departmentId, year, month);

        userRuleRepositoryMock.Setup(r => r.GetUsersRulesByDepartment(departmentId, "december", year))
                .ReturnsAsync(new List<UserScheduleRules>());

        // Act
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Schedule rules not found");

        userRuleRepositoryMock.Verify(r => r.GetUsersRulesByDepartment(departmentId, "december", year), Times.Once);
        scheduleRepositoryMock.Verify(r => r.GetMonthSchedule(It.IsAny<string>()), Times.Never);
    }
}