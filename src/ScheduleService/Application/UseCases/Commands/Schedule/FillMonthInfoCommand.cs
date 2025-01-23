namespace ScheduleService.Application.UseCases.Commands.Schedule
{
    using MediatR;

    public record FillMonthInfoCommand : IRequest
    {
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
