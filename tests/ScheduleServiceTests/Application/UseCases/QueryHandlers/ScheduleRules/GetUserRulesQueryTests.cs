using FluentAssertions;
using MongoDB.Bson;
using Moq;
using ScheduleService.Application.UseCases.Queries.ScheduleRules;
using ScheduleService.Application.UseCases.QueryHandlers.ScheduleRules;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Models;
using Xunit;

namespace Application.UseCases.QueryHandlers.ScheduleRules;

public class GetUserRulesQueryTests
{
    private readonly Mock<IUserRuleRepository> userRuleRepositoryMock;
    private readonly GetUserRulesQueryHandler hander;

    public GetUserRulesQueryTests()
    {
        userRuleRepositoryMock = new Mock<IUserRuleRepository>();
        hander = new GetUserRulesQueryHandler(userRuleRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsUserRules()
    {
        // Arrange
        var userId = ObjectId.GenerateNewId().ToString();
        var departmentId = ObjectId.GenerateNewId().ToString();
        var monthName = "january";
        var year = 2025;

        var rules = new UserScheduleRules()
        {
            Id = ObjectId.GenerateNewId().ToString(),
        };

        var query = new GetUserRulesQuery(userId, departmentId, 1, year);

        userRuleRepositoryMock.Setup(
            x => x.GetMonthScheduleRules(userId, departmentId, monthName, year))
            .ReturnsAsync(rules);

        // Act
        var result = await hander.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(rules);
        userRuleRepositoryMock.Verify(r => r.GetMonthScheduleRules(userId, departmentId, monthName, year), Times.Once);
    }

    [Fact]
    public async Task Handle_Throws_NotFoundException()
    {
        // Arrange
        var userId = ObjectId.GenerateNewId().ToString();
        var departmentId = ObjectId.GenerateNewId().ToString();
        var monthName = "january";
        var year = 2025;

        var rules = new UserScheduleRules() { };

        var query = new GetUserRulesQuery(userId, departmentId, 1, year);

        userRuleRepositoryMock.Setup(
                x => x.GetMonthScheduleRules(userId, departmentId, monthName, year))
            .ReturnsAsync((UserScheduleRules)null);

        // Act
        var act = async () => hander.Handle(query, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Rules not found");
    }
}