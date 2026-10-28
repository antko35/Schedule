namespace Application.UseCases.QueryHandlers.Schedule;

using Application;
using ScheduleService.Application.UseCases.Queries.Schedule;
using ScheduleService.Application.UseCases.QueryHandlers.Schedule;

public sealed class GetUserMonthScheduleQueryTests
{
    [Fact]
    public async Task Handle_RejectsMissingMonthlySchedule()
    {
        var repository = new Mock<IScheduleRepository>();
        repository.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MonthlySchedule?)null);
        var handler = new GetUserMonthScheduleQueryHandler(repository.Object);

        var action = () => handler.Handle(
            new GetUserMonthScheduleQuery("user-id", "department-id", 2026, 10),
            CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Invalid input");
    }

    [Fact]
    public async Task Handle_ReturnsMappedMonthlySchedule()
    {
        var repository = new Mock<IScheduleRepository>();
        var schedule = TestSupport.CreateSchedule();
        schedule.SetShift(TestSupport.CreateShift(day: 7), TestSupport.Utc(2026, 9, 2));
        repository.Setup(x => x.GetAsync("user-id", "department-id", TestSupport.Period, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        var handler = new GetUserMonthScheduleQueryHandler(repository.Object);

        var result = await handler.Handle(
            new GetUserMonthScheduleQuery("user-id", "department-id", 2026, 10),
            CancellationToken.None);

        result.Id.Should().Be(schedule.Id);
        result.MonthName.Should().Be("october");
        result.WorkDays.Should().ContainSingle().Which.Day.Should().Be(7);
    }
}
