namespace ScheduleService.Application.UseCases.CommandHandlers.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Application.UseCases.Commands.Calendar;
    using ScheduleService.Application.UseCases.Commands.Schedule;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class GetMonthHolidaysCommandHandler : IRequestHandler<GetMonthHolidaysCommand, List<Calendar>>
    {
        private readonly ICalendarRepository calendarRepository;

        public GetMonthHolidaysCommandHandler(
            ICalendarRepository calendarRepository)
        {
            this.calendarRepository = calendarRepository;
        }

        public async Task<List<Calendar>> Handle(GetMonthHolidaysCommand request, CancellationToken cancellationToken)
        {
            var response = await calendarRepository.GetMonthHolidays(request.Year, request.Month);

            return response;
        }
    }
}
