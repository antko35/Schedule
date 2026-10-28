namespace Application.UseCases.QueryHandlers.Calendar;

using ScheduleService.Application.UseCases.Queries.Calendar;
using ScheduleService.Application.UseCases.QueryHandlers.Calendar;

public sealed class GetMonthTransferDaysQueryTests
{
    [Fact]
    public async Task Handle_MapsMonthlyTransferDays()
    {
        var repository = new Mock<ICalendarRepository>();
        var transfer = new CalendarException(
            "holiday-id",
            new DateOnly(2026, 9, 30),
            new DateOnly(2026, 10, 3));
        repository.Setup(x => x.GetMonthTransferDaysAsync(new DateOnly(2026, 10, 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync([transfer]);
        var handler = new GetMonthTransferDaysQueryHandler(repository.Object);

        var result = await handler.Handle(new GetMonthTransferDaysQuery(2026, 10), CancellationToken.None);

        result.Should().ContainSingle().Which.TransferDate.Should().Be(transfer.TransferDate);
    }
}
