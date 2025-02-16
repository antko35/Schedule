using AutoMapper;
using FluentAssertions;
using Moq;
using ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Models;
using Xunit;

namespace Application.UseCases.CommandHandlers.ScheduleRules;

public class SetGenerationRulesTests
{
    private readonly Mock<IUserRuleRepository> mockUserRuleRepository;
    private readonly IMapper mapper;
    private readonly SetGenerationRulesCommandHandler handler;

    public SetGenerationRulesTests()
    {
        this.mockUserRuleRepository = new Mock<IUserRuleRepository>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<ScheduleService.Application.Mapping.ScheduleRules>());
        this.mapper = config.CreateMapper();

        this.handler = new SetGenerationRulesCommandHandler(mockUserRuleRepository.Object, mapper);
    }

    [Fact]
    public async Task Handle_ShouldUpdateScheduleRules_WhenRulesExist()
    {
        // Arrange
        var command = new SetGenerationRulesCommand { ScheduleRulesId = "rule123", EvenDOW = true };
        var existingRules = new UserScheduleRules { Id = command.ScheduleRulesId, OnlyFirstShift = true };

        mockUserRuleRepository.Setup(repo => repo.GetByIdAsync(command.ScheduleRulesId))
            .ReturnsAsync(existingRules);
        mockUserRuleRepository.Setup(repo => repo.UpdateAsync(existingRules))
            .Returns(Task.CompletedTask);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        existingRules.EvenDOW.Should().BeTrue();
        existingRules.OnlyFirstShift.Should().BeFalse();

        mockUserRuleRepository.Verify(repo => repo.UpdateAsync(existingRules), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRulesNotFound()
    {
        // Arrange
        var command = new SetGenerationRulesCommand { ScheduleRulesId = "invalidId" };
        mockUserRuleRepository.Setup(repo => repo.GetByIdAsync(command.ScheduleRulesId))
            .ReturnsAsync((UserScheduleRules)null);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Schedule rules not found");
    }
}
