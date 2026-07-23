namespace ScheduleService.Application.UseCases.QueryHandlers.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Application.UseCases.Queries.Calendar;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class GetMonthTransferDaysQueryHandler
        : IRequestHandler<GetMonthTransferDaysQuery, List<Calendar>>
    {
        private readonly ICalendarRepository calendarRepository;

        public GetMonthTransferDaysQueryHandler(ICalendarRepository calendarRepository)
        {
            this.calendarRepository = calendarRepository;
        }

        public async Task<List<Calendar>> Handle(GetMonthTransferDaysQuery request, CancellationToken cancellationToken)
        {
            var result = await calendarRepository.GetMonthTransferDays(request.Year, request.Month);

            return result;
        }
    }
}
