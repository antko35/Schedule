using AutoMapper;

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
        private readonly IMapper mapper;

        public AddOfficialHolidayCommandHandler(ICalendarRepository calendarRepository, IMapper mapper)
        {
            this.calendarRepository = calendarRepository;
            this.mapper = mapper;
        }

        public async Task<Calendar> Handle(AddOfficialHolidayCommand request, CancellationToken cancellationToken)
        {
            await IsHolidayAlreadyExist(request.Holiday);

            var holidayDay = mapper.Map<Calendar>(request);

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
    }
}
