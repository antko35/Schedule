using FluentAssertions;
using Moq;
using ScheduleService.Application.UseCases.CommandHandlers.Schedule;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;
using Xunit;
using Range = Moq.Range;

namespace Application.UseCases.CommandHandlers.Schedule;

public class AddWorkingSaturdayCommandTests
{
    private readonly Mock<IScheduleRepository> scheduleRepositoryMock;
    private readonly AddWorkingSaturdayCommandHandler handler;

    public AddWorkingSaturdayCommandTests()
    {
        scheduleRepositoryMock = new Mock<IScheduleRepository>();
        handler = new AddWorkingSaturdayCommandHandler(scheduleRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldAddSaturday_WhenScheduleExistsAndSaturdayIsValid()
    {
        // Arrange
        var scheduleId = "schedule-1";
        var saturday = new WorkDay
        {
            Day = 6,
            StartTime = new DateTime(2025, 3, 6,  9, 0, 0),
            EndTime = new DateTime(2025, 3, 6,  14, 0, 0),
        };
        var newWorkingDays = new List<WorkDay>
        {
            new WorkDay
            {
                Day = 1,
                StartTime = new DateTime(2025, 3, 1,  9, 0, 0),
                EndTime = new DateTime(2025, 3, 1,  14, 30, 0),
            },
            new WorkDay
            {
                Day = 2,
                StartTime = new DateTime(2025, 3, 2,  9, 0, 0),
                EndTime = new DateTime(2025, 3, 2,  14, 30, 0),
            },
            new WorkDay
            {
                Day = 3,
                StartTime = new DateTime(2025, 3, 3,  9, 0, 0),
                EndTime = new DateTime(2025, 3, 3,  14, 30, 0),
            },
            new WorkDay
            {
                Day = 4,
                StartTime = new DateTime(2025, 3, 4,  10, 0, 0),
                EndTime = new DateTime(2025, 3, 4,  15, 30, 0),
            },
            new WorkDay
            {
                Day = 5,
                StartTime = new DateTime(2025, 3, 5,  10, 0, 0),
                EndTime = new DateTime(2025, 3, 5,  15, 30, 0),
            },
        };

        var workingDays = new List<ScheduleService.Domain.Models.Schedule>
        {
            new ScheduleService.Domain.Models.Schedule()
            {
                WorkDays = new List<WorkDay>()
                {
                    new WorkDay()
                    {
                        Day = 1,
                        StartTime = new DateTime(2025, 3, 1,  9, 0, 0),
                        EndTime = new DateTime(2025, 3, 1,  15, 30, 0),
                    },
                },
            },
            new ScheduleService.Domain.Models.Schedule()
            {
                WorkDays = new List<WorkDay>()
                {
                    new WorkDay()
                    {
                        Day = 2,
                        StartTime = new DateTime(2025, 3, 2,  9, 0, 0),
                        EndTime = new DateTime(2025, 3, 2,  15, 30, 0),
                    },
                },
            },
            new ScheduleService.Domain.Models.Schedule()
            {
                WorkDays = new List<WorkDay>()
                {
                    new WorkDay()
                    {
                        Day = 3,
                        StartTime = new DateTime(2025, 3, 3,  9, 0, 0),
                        EndTime = new DateTime(2025, 3, 3,  15, 30, 0),
                    },
                },
            },
            new ScheduleService.Domain.Models.Schedule()
            {
                WorkDays = new List<WorkDay>()
                {
                    new WorkDay()
                    {
                        Day = 4,
                        StartTime = new DateTime(2025, 3, 4,  9, 0, 0),
                        EndTime = new DateTime(2025, 3, 4,  15, 30, 0),
                    },
                },
            },
            new ScheduleService.Domain.Models.Schedule()
            {
                WorkDays = new List<WorkDay>()
                {
                    new WorkDay()
                    {
                        Day = 5,
                        StartTime = new DateTime(2025, 3, 5,  9, 0, 0),
                        EndTime = new DateTime(2025, 3, 5,  15, 30, 0),
                    },
                },
            },
        };

        var command = new AddWorkingSaturdayCommand
        {
            ScheduleId = scheduleId,
            Saturday = saturday,
            WorkingDays = newWorkingDays,
        };

        scheduleRepositoryMock
            .Setup(repo => repo.GetByIdAsync(scheduleId))
            .ReturnsAsync(new ScheduleService.Domain.Models.Schedule());

        scheduleRepositoryMock
            .Setup(repo => repo.GetWorkDayAsync(scheduleId, 6))
            .ReturnsAsync((ScheduleService.Domain.Models.Schedule)null);

        foreach (var workDay in workingDays)
        {
            scheduleRepositoryMock
                .Setup(repo => repo.GetWorkDayAsync(scheduleId, It.IsInRange(1, 5, Range.Inclusive)))
                .ReturnsAsync(workDay);
        }

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        scheduleRepositoryMock.Verify(repo => repo.AddWorkDayAsync(scheduleId, saturday), Times.Once);
        scheduleRepositoryMock.Verify(repo => repo.UpdateWorkDaysAsync(scheduleId, newWorkingDays), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenScheduleDoesNotExist()
    {
        // Arrange
        var scheduleId = "schedule-1";
        var command = new AddWorkingSaturdayCommand { ScheduleId = scheduleId };

        scheduleRepositoryMock
            .Setup(repo => repo.GetByIdAsync(scheduleId))
            .ReturnsAsync((ScheduleService.Domain.Models.Schedule)null);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Schedule not found");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenSaturdayAlreadyExists()
    {
        // Arrange
        var scheduleId = "schedule-1";
        var saturday = new ScheduleService.Domain.Models.Schedule { };

        var command = new AddWorkingSaturdayCommand
        {
            ScheduleId = scheduleId,
            Saturday = new WorkDay()
            {
                Day = 1,
            },
        };

        scheduleRepositoryMock
            .Setup(repo => repo.GetByIdAsync(scheduleId))
            .ReturnsAsync(new ScheduleService.Domain.Models.Schedule());

        scheduleRepositoryMock
            .Setup(repo => repo.GetWorkDayAsync(scheduleId, It.IsAny<int>()))
            .ReturnsAsync(saturday);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Saturday already exist");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenWorkingDaysCountIsNotFive()
    {
        // Arrange
        var scheduleId = "schedule-1";
        var saturday = new WorkDay { Day = 1 };
        var workingDays = new List<WorkDay> { new WorkDay() };

        var command = new AddWorkingSaturdayCommand
        {
            ScheduleId = scheduleId,
            Saturday = saturday,
            WorkingDays = workingDays,
        };

        scheduleRepositoryMock
            .Setup(repo => repo.GetByIdAsync(scheduleId))
            .ReturnsAsync(new ScheduleService.Domain.Models.Schedule()); // Schedule exists

        scheduleRepositoryMock
            .Setup(repo => repo.GetWorkDayAsync(scheduleId, It.IsAny<int>()))
            .ReturnsAsync((ScheduleService.Domain.Models.Schedule)null); // Saturday does not exist

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Should be 5 updated days");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenWorkDayLengthIsLessThan4Hours()
    {
        // Arrange
        var scheduleId = "schedule-1";
        var saturday = new WorkDay { Day = 6 };
        var newWorkingDays = new List<WorkDay>
        {
            new WorkDay
            {
                Day = 1,
                StartTime = new DateTime(2025, 3, 1,  9, 0, 0),
                EndTime = new DateTime(2025, 3, 1,  10, 30, 0),
            },
            new WorkDay
            {
                Day = 2,
                StartTime = new DateTime(2025, 3, 2,  9, 0, 0),
                EndTime = new DateTime(2025, 3, 2,  14, 30, 0),
            },
            new WorkDay
            {
                Day = 3,
                StartTime = new DateTime(2025, 3, 3,  9, 0, 0),
                EndTime = new DateTime(2025, 3, 3,  14, 30, 0),
            },
            new WorkDay
            {
                Day = 4,
                StartTime = new DateTime(2025, 3, 4,  10, 0, 0),
                EndTime = new DateTime(2025, 3, 4,  15, 30, 0),
            },
            new WorkDay
            {
                Day = 5,
                StartTime = new DateTime(2025, 3, 5,  10, 0, 0),
                EndTime = new DateTime(2025, 3, 5,  15, 30, 0),
            },
        };
        var workingDays = new List<ScheduleService.Domain.Models.Schedule>
        {
            new ScheduleService.Domain.Models.Schedule()
            {
                WorkDays = new List<WorkDay>()
                {
                    new WorkDay()
                    {
                        Day = 1,
                        StartTime = new DateTime(2025, 3, 1,  9, 0, 0),
                        EndTime = new DateTime(2025, 3, 1,  15, 30, 0),
                    },
                },
            },
            new ScheduleService.Domain.Models.Schedule()
            {
                WorkDays = new List<WorkDay>()
                {
                    new WorkDay()
                    {
                        Day = 2,
                        StartTime = new DateTime(2025, 3, 2,  9, 0, 0),
                        EndTime = new DateTime(2025, 3, 2,  15, 30, 0),
                    },
                },
            },
            new ScheduleService.Domain.Models.Schedule()
            {
                WorkDays = new List<WorkDay>()
                {
                    new WorkDay()
                    {
                        Day = 3,
                        StartTime = new DateTime(2025, 3, 3,  9, 0, 0),
                        EndTime = new DateTime(2025, 3, 3,  15, 30, 0),
                    },
                },
            },
            new ScheduleService.Domain.Models.Schedule()
            {
                WorkDays = new List<WorkDay>()
                {
                    new WorkDay()
                    {
                        Day = 4,
                        StartTime = new DateTime(2025, 3, 4,  9, 0, 0),
                        EndTime = new DateTime(2025, 3, 4,  15, 30, 0),
                    },
                },
            },
            new ScheduleService.Domain.Models.Schedule()
            {
                WorkDays = new List<WorkDay>()
                {
                    new WorkDay()
                    {
                        Day = 5,
                        StartTime = new DateTime(2025, 3, 5,  9, 0, 0),
                        EndTime = new DateTime(2025, 3, 5,  15, 30, 0),
                    },
                },
            },
        };

        var command = new AddWorkingSaturdayCommand
        {
            ScheduleId = scheduleId,
            Saturday = saturday,
            WorkingDays = newWorkingDays,
        };

        scheduleRepositoryMock
            .Setup(repo => repo.GetByIdAsync(scheduleId))
            .ReturnsAsync(new ScheduleService.Domain.Models.Schedule()); // Schedule exists

        scheduleRepositoryMock
            .Setup(repo => repo.GetWorkDayAsync(scheduleId, 6))
            .ReturnsAsync((ScheduleService.Domain.Models.Schedule)null); // Saturday does not exist

        foreach (var workDay in workingDays)
        {
            scheduleRepositoryMock
                .Setup(repo => repo.GetWorkDayAsync(scheduleId, It.IsInRange(1, 5, Range.Inclusive)))
                .ReturnsAsync(workDay);
        }

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Minimum work day length is 4 hours");
    }
}