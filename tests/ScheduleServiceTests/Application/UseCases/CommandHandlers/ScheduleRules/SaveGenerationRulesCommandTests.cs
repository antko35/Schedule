using MongoDB.Bson;
using Moq;
using ScheduleService.Application.Services;
using ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;
using Xunit;

namespace Application.UseCases.CommandHandlers.ScheduleRules;

public class SaveGenerationRulesCommandTests
{
    private readonly Mock<IUserRuleRepository> userRuleRepository = new Mock<IUserRuleRepository>();
    private readonly Mock<IScheduleRepository> scheduleRepository = new Mock<IScheduleRepository>();

    [Fact]
    public async Task Handle_CreatesMonthlyContextAndPersistsEnteredValues()
    {
        var command = CreateCommand();
        var handler = CreateHandler();

        var result = await handler.Handle(command, CancellationToken.None);

        scheduleRepository.Verify(
            repository => repository.AddAsync(It.Is<ScheduleService.Domain.Models.Schedule>(schedule =>
                schedule.Id == result.ScheduleId
                && schedule.MonthName == "october"
                && schedule.WorkDays.Count == 0)),
            Times.Once);
        userRuleRepository.Verify(
            repository => repository.AddAsync(It.Is<UserScheduleRules>(rules =>
                rules.ScheduleId == result.ScheduleId
                && rules.HoursPerMonth == 160
                && rules.MaxHoursPerDay == 8
                && rules.StartWorkDayTime == new TimeOnly(8, 0)
                && rules.OnlyFirstShift)),
            Times.Once);
        userRuleRepository.Verify(
            repository => repository.UpdateAsync(It.IsAny<UserScheduleRules>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UpdatesExistingRulesWithoutCreatingAnotherSchedule()
    {
        var command = CreateCommand();
        var existingRules = new UserScheduleRules
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = command.UserId,
            DepartmentId = command.DepartmentId,
            Year = command.Year,
            MonthName = "october",
            ScheduleId = ObjectId.GenerateNewId().ToString(),
        };
        userRuleRepository
            .Setup(repository => repository.GetMonthScheduleRules(
                command.UserId,
                command.DepartmentId,
                "october",
                command.Year))
            .ReturnsAsync(existingRules);
        scheduleRepository
            .Setup(repository => repository.GetByIdAsync(existingRules.ScheduleId))
            .ReturnsAsync(new ScheduleService.Domain.Models.Schedule { Id = existingRules.ScheduleId });

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.Same(existingRules, result);
        scheduleRepository.Verify(
            repository => repository.AddAsync(It.IsAny<ScheduleService.Domain.Models.Schedule>()),
            Times.Never);
        userRuleRepository.Verify(
            repository => repository.AddAsync(It.IsAny<UserScheduleRules>()),
            Times.Never);
        userRuleRepository.Verify(repository => repository.UpdateAsync(existingRules), Times.Once);
    }

    [Fact]
    public async Task Handle_RemovesNewSchedule_WhenRulesCannotBeCreated()
    {
        var command = CreateCommand();
        scheduleRepository
            .Setup(repository => repository.AddAsync(It.IsAny<ScheduleService.Domain.Models.Schedule>()))
            .Returns(Task.CompletedTask);
        userRuleRepository
            .Setup(repository => repository.AddAsync(It.IsAny<UserScheduleRules>()))
            .ThrowsAsync(new InvalidOperationException("write failed"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));

        scheduleRepository.Verify(
            repository => repository.RemoveAsync(It.IsAny<string>()),
            Times.Once);
        userRuleRepository.Verify(
            repository => repository.UpdateAsync(It.IsAny<UserScheduleRules>()),
            Times.Never);
    }

    private SaveGenerationRulesCommandHandler CreateHandler()
    {
        var monthContextService = new ScheduleMonthContextService(
            userRuleRepository.Object,
            scheduleRepository.Object);
        return new SaveGenerationRulesCommandHandler(monthContextService, userRuleRepository.Object);
    }

    private static SaveGenerationRulesCommand CreateCommand()
    {
        return new SaveGenerationRulesCommand(
            160,
            8,
            new TimeOnly(8, 0),
            true,
            false,
            false,
            false,
            false,
            false)
        {
            UserId = ObjectId.GenerateNewId().ToString(),
            DepartmentId = ObjectId.GenerateNewId().ToString(),
            Year = 2026,
            Month = 10,
        };
    }
}
