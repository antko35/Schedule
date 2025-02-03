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
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class GetMonthTransferDaysCommandHandler 
        : IRequestHandler<GetMonthTransferDaysCommand, List<Calendar>>
    {
        private readonly ICalendarRepository calendarRepository;

        public GetMonthTransferDaysCommandHandler(ICalendarRepository calendarRepository)
        {
            this.calendarRepository = calendarRepository;
        }

        public async Task<List<Calendar>> Handle(GetMonthTransferDaysCommand request, CancellationToken cancellationToken)
        {
            var result = await calendarRepository.GetMonthTransferDays(request.Year, request.Month);

            return result;
        }
    }
}
