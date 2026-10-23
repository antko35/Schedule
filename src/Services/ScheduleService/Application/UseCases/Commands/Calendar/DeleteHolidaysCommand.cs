namespace ScheduleService.Application.UseCases.Commands.Calendar
{
    using MediatR;

    public record DeleteHolidaysCommand(IReadOnlyCollection<DateOnly> Holidays)
        : IRequest;
}
