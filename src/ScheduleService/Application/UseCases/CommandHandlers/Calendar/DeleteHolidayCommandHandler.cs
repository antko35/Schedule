using MediatR;
using ScheduleService.Application.UseCases.Commands.Calendar;
using ScheduleService.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.UseCases.CommandHandlers.Calendar
{
    public class DeleteHolidayCommandHandler
        : IRequestHandler<DeleteHolidayCommand>
    {
        private readonly ICalendarRepository calendarRepository;

        public DeleteHolidayCommandHandler(ICalendarRepository calendarRepository)
        {
            this.calendarRepository = calendarRepository;
        }

        public async Task Handle(DeleteHolidayCommand request, CancellationToken cancellationToken)
        {
            var holidays = await calendarRepository.GetMonthHolidays(request.Holiday.Year, request.Holiday.Month);

            var holidayToDelete = holidays.FirstOrDefault(day => day.HolidayDate == request.Holiday);

            if (holidayToDelete != null)
            {
                await calendarRepository.RemoveAsync(holidayToDelete.Id);
            }
            else
            {
                throw new KeyNotFoundException("Day not found");
            }
        }
    }
}
