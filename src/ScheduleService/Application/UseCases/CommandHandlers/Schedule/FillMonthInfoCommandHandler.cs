using MediatR;
using ScheduleService.Application.UseCases.Commands.Schedule;

namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule
{
    public class FillMonthInfoCommandHandler : IRequestHandler<FillMonthInfoCommand>
    {
        public Task Handle(FillMonthInfoCommand request, CancellationToken cancellationToken)
        {
            int daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);
            DateTime firstDayOfMonth = new DateTime(request.Year, request.Month, 1);
            DayOfWeek startDayOfWeek = firstDayOfMonth.DayOfWeek;

            throw new NotImplementedException();
        }
    }
}
