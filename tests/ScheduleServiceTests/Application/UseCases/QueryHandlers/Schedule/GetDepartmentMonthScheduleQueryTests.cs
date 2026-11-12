namespace Application.UseCases.QueryHandlers.Schedule;

using Application;
using ScheduleService.Application.UseCases.Queries.Schedule;
using ScheduleService.Application.UseCases.QueryHandlers.Schedule;

public sealed class GetDepartmentMonthScheduleQueryTests
{
    [Fact]
    public async Task Handle_ReturnsAllDepartmentSchedulesForPeriod()
    {
        var repository = new Mock<IScheduleRepository>();
        var first = TestSupport.CreateSchedule("schedule-1");
        var second = new MonthlySchedule(
            "schedule-2",
            "user-2",
            "department-id",
            TestSupport.Period,
            TestSupport.Utc(2026, 9, 1));
        repository.Setup(x => x.GetDepartmentAsync("department-id", TestSupport.Period, It.IsAny<CancellationToken>()))
            .ReturnsAsync([first, second]);
        var handler = new GetDepartmentMonthScheduleQueryHandler(repository.Object);

        var result = await handler.Handle(
            new GetDepartmentMonthScheduleQuery("department-id", 2026, 10),
            CancellationToken.None);

        result.Select(item => item.Id).Should().Equal("schedule-1", "schedule-2");
    }

    [Fact]
    public async Task Handle_RejectsMissingDepartmentSchedules()
    {
        var repository = new Mock<IScheduleRepository>();
        repository.Setup(x => x.GetDepartmentAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<MonthlySchedule>());
        var handler = new GetDepartmentMonthScheduleQueryHandler(repository.Object);

        var action = () => handler.Handle(
            new GetDepartmentMonthScheduleQuery("department-id", 2026, 10),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>();
    }
}
