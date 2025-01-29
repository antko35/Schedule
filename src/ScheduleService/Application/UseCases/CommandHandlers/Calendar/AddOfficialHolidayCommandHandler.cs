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
        : IRequestHandler<AddOfficialHolidayCommand, Calendar>
    {
        private readonly ICalendarRepository calendarRepository;

        public AddOfficialHolidayCommandHandler(ICalendarRepository calendarRepository)
        {
            this.calendarRepository = calendarRepository;
        }

        public async Task<Calendar> Handle(AddOfficialHolidayCommand request, CancellationToken cancellationToken)
        {
            DayOfWeek dayOfWeek = request.Holiday.DayOfWeek;

            var holidayDay = new Calendar()
            {
                HolidayDayOfMonth = request.Holiday.Day,
                DayOfWeek = dayOfWeek,
                HolidayDate = request.Holiday,
                MonthOfHoliday = request.Holiday.Month,
                MonthOfTransferDay = request.TransferDay.Month,
            };

            if (request.TransferDay.Year == 0001)
            {
                holidayDay.TransferDate = null;            }
            else
            {
                holidayDay.TransferDate = request.TransferDay;
            }

            await calendarRepository.AddAsync(holidayDay);

            return holidayDay;
        }
    }
}
