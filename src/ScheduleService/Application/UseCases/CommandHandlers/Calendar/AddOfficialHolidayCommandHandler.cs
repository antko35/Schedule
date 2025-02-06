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
            await IsHolidayAlreadyExist(request.Holiday);

            var holidayDay = await CreateDay(request);

            await calendarRepository.AddAsync(holidayDay);

            return holidayDay;
        }

        private async Task IsHolidayAlreadyExist(DateOnly holiday)
        {
            var holidays = await calendarRepository.GetMonthHolidays(holiday.Year, holiday.Month);

            if (holidays.Any(x => x.HolidayDate.Day == holiday.Day))
            {
                throw new InvalidOperationException($"Holiday {holiday} already exist");
            }
        }

        private async Task<Calendar> CreateDay(AddOfficialHolidayCommand request)
        {
            DayOfWeek dayOfWeek = request.Holiday.DayOfWeek;
            var holidayDay = new Calendar()
            {
                Year = request.Holiday.Year,
                HolidayDayOfMonth = request.Holiday.Day,
                DayOfWeek = dayOfWeek,
                HolidayDate = request.Holiday,
                MonthOfHoliday = request.Holiday.Month,
                MonthOfTransferDay = request.TransferDay.Month,
            };

            if (request.TransferDay.Year == 0001)
            {
                holidayDay.TransferDate = null;
            }
            else
            {
                holidayDay.TransferDate = request.TransferDay;
            }

            return holidayDay;
        }
    }
}
