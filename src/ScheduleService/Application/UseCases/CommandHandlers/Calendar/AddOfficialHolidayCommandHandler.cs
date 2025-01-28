namespace ScheduleService.Application.UseCases.CommandHandlers.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Application.UseCases.Commands.Calendar;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class AddOfficialHolidayCommandHandler
        : IRequestHandler<AddOfficialHolidayCommand>
    {
        private readonly ICalendarRepository calendarRepository;

        public AddOfficialHolidayCommandHandler(ICalendarRepository calendarRepository)
        {
            this.calendarRepository = calendarRepository;
        }

        public async Task Handle(AddOfficialHolidayCommand request, CancellationToken cancellationToken)
        {
            DayOfWeek dayOfWeek = request.Holiday.DayOfWeek;

            var holidayDay = new Calendar()
            {
                DayOfMonth = request.Holiday.Day,
                DayOfWeek = dayOfWeek,
                OfficialHoliday = true,
                Holiday = request.Holiday,
                TransferDay = request.TransferDay,
            };

            await calendarRepository.AddAsync(holidayDay);
        }
    }
}
