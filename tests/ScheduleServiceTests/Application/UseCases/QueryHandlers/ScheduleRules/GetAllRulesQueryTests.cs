using FluentAssertions;
using MongoDB.Bson;
using Moq;
using ScheduleService.Application.UseCases.QueryHandlers.ScheduleRules;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Models;
using Xunit;

namespace Application.UseCases.QueryHandlers.ScheduleRules;

public class GetAllRulesQueryTests
{
    private readonly Mock<IUserRuleRepository> userRuleRepositoryMock;
    private readonly GetAllRulesQueryHandler hander;

    public GetAllRulesQueryTests()
    {
        userRuleRepositoryMock = new Mock<IUserRuleRepository>();
        hander = new GetAllRulesQueryHandler(userRuleRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsRules()
    {
        // Arrange
        var rules = new List<UserScheduleRules>()
        {
            new UserScheduleRules() { Id = ObjectId.GenerateNewId().ToString() },
            new UserScheduleRules() { Id = ObjectId.GenerateNewId().ToString() },
        };

        userRuleRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(rules);

        // Act
        var result = await hander.Handle(null, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(rules);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList()
    {
        // Arrange
        var rules = new List<UserScheduleRules>()
        {
        };

        userRuleRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(rules);

        // Act
        var result = await hander.Handle(null, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(rules);
    }
}