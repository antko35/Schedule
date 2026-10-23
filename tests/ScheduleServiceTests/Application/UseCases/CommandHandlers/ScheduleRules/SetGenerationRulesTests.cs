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
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(existingRules);
        existingRules.EvenDOW.Should().BeTrue();
        existingRules.OnlyFirstShift.Should().BeFalse();

        mockUserRuleRepository.Verify(repo => repo.UpdateAsync(existingRules), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldClearAllPeriodicityFlags_WhenAllFlagsAreExplicitlyFalse()
    {
        // Arrange
        var command = new SetGenerationRulesCommand
        {
            ScheduleRulesId = "rule123",
            OnlyFirstShift = false,
            OnlySecondShift = false,
            EvenDOW = false,
            UnEvenDOW = false,
            EvenDOM = false,
            UnEvenDOM = false,
        };
        var existingRules = new UserScheduleRules
        {
            Id = command.ScheduleRulesId,
            EvenDOM = true,
        };

        mockUserRuleRepository.Setup(repo => repo.GetByIdAsync(command.ScheduleRulesId))
            .ReturnsAsync(existingRules);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        existingRules.OnlyFirstShift.Should().BeFalse();
        existingRules.OnlySecondShift.Should().BeFalse();
        existingRules.EvenDOW.Should().BeFalse();
        existingRules.UnEvenDOW.Should().BeFalse();
        existingRules.EvenDOM.Should().BeFalse();
        existingRules.UnEvenDOM.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldPreservePeriodicityFlags_WhenAllFlagsAreOmitted()
    {
        // Arrange
        var command = new SetGenerationRulesCommand { ScheduleRulesId = "rule123" };
        var existingRules = new UserScheduleRules
        {
            Id = command.ScheduleRulesId,
            UnEvenDOW = true,
        };

        mockUserRuleRepository.Setup(repo => repo.GetByIdAsync(command.ScheduleRulesId))
            .ReturnsAsync(existingRules);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        existingRules.UnEvenDOW.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldSetMidnightStartTime_WhenExplicitlyProvided()
    {
        // Arrange
        var command = new SetGenerationRulesCommand
        {
            ScheduleRulesId = "rule123",
            StartWorkDayTime = TimeOnly.MinValue,
        };
        var existingRules = new UserScheduleRules
        {
            Id = command.ScheduleRulesId,
            StartWorkDayTime = new TimeOnly(8, 0),
        };

        mockUserRuleRepository.Setup(repo => repo.GetByIdAsync(command.ScheduleRulesId))
            .ReturnsAsync(existingRules);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        existingRules.StartWorkDayTime.Should().Be(TimeOnly.MinValue);
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
