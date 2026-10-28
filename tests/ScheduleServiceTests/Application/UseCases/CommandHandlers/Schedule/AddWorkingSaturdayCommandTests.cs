namespace Application.UseCases.CommandHandlers.Schedule;

using Application;
using ScheduleService.Application.Contracts;
using ScheduleService.Application.UseCases.CommandHandlers.Schedule;
using ScheduleService.Application.UseCases.Commands.Schedule;

public sealed class AddWorkingSaturdayCommandTests
{
    [Fact]
    public async Task Handle_RejectsSaturdayThatAlreadyExists()
    {
        var repository = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var schedule = TestSupport.CreateSchedule();
        schedule.SetShift(TestSupport.CreateShift(day: 10), TestSupport.Utc(2026, 9, 2));
        repository.Setup(x => x.GetByIdAsync(schedule.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        var handler = new AddWorkingSaturdayCommandHandler(repository.Object, dbContext.Object);

        var action = () => handler.Handle(
            new AddWorkingSaturdayCommand
            {
                ScheduleId = schedule.Id,
                Saturday = Day(10, 8, 12),
                WorkingDays = [],
            },
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Saturday already exist");
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RequiresExactlyFiveWeekdays()
    {
        var repository = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var schedule = TestSupport.CreateSchedule();
        repository.Setup(x => x.GetByIdAsync(schedule.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        var handler = new AddWorkingSaturdayCommandHandler(repository.Object, dbContext.Object);

        var action = () => handler.Handle(
            new AddWorkingSaturdayCommand
            {
                ScheduleId = schedule.Id,
                Saturday = Day(10, 8, 12),
                WorkingDays = [Day(5, 8, 16)],
            },
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Should be 5 updated days");
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RejectsUnbalancedSaturdayHours()
    {
        var repository = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var schedule = TestSupport.CreateSchedule();
        foreach (var day in Enumerable.Range(5, 5))
        {
            schedule.SetShift(TestSupport.CreateShift(day: day), TestSupport.Utc(2026, 9, 2));
        }

        repository.Setup(x => x.GetByIdAsync(schedule.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        var handler = new AddWorkingSaturdayCommandHandler(repository.Object, dbContext.Object);

        var action = () => handler.Handle(
            new AddWorkingSaturdayCommand
            {
                ScheduleId = schedule.Id,
                Saturday = Day(10, 8, 13),
                WorkingDays = Enumerable.Range(5, 5).Select(day => Day(day, 8, 15, 12)).ToList(),
            },
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid time difference*");
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RebalancesWeekAndAddsSaturday()
    {
        var repository = new Mock<IScheduleRepository>();
        var dbContext = TestSupport.CreateDbContextMock();
        var schedule = TestSupport.CreateSchedule();
        foreach (var day in Enumerable.Range(5, 5))
        {
            schedule.SetShift(TestSupport.CreateShift(day: day), TestSupport.Utc(2026, 9, 2));
        }

        repository.Setup(x => x.GetByIdAsync(schedule.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        var handler = new AddWorkingSaturdayCommandHandler(repository.Object, dbContext.Object);
        var shortenedDays = Enumerable.Range(5, 5)
            .Select(day => new WorkDayDto
            {
                Day = day,
                StartTime = TestSupport.Utc(2026, 10, day, 8),
                EndTime = TestSupport.Utc(2026, 10, day, 15, 12),
            })
            .ToList();
        var command = new AddWorkingSaturdayCommand
        {
            ScheduleId = schedule.Id,
            Saturday = new WorkDayDto
            {
                Day = 10,
                StartTime = TestSupport.Utc(2026, 10, 10, 8),
                EndTime = TestSupport.Utc(2026, 10, 10, 12),
            },
            WorkingDays = shortenedDays,
        };

        await handler.Handle(command, CancellationToken.None);

        schedule.WorkShifts.Should().HaveCount(6);
        schedule.WorkShifts.Should().Contain(shift => shift.WorkDate == new DateOnly(2026, 10, 10));
        dbContext.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    private static WorkDayDto Day(int day, int startHour, int endHour, int endMinute = 0) =>
        new()
        {
            Day = day,
            StartTime = TestSupport.Utc(2026, 10, day, startHour),
            EndTime = TestSupport.Utc(2026, 10, day, endHour, endMinute),
        };
}
