namespace ScheduleService.Application.UseCases.CommandHandlers.Calendar
{
    using MediatR;
    using ScheduleService.Application.UseCases.Commands.Calendar;
    using ScheduleService.Domain.Abstractions;

    public class DeleteHolidaysCommandHandler
        : IRequestHandler<DeleteHolidaysCommand>
    {
        private readonly ICalendarRepository calendarRepository;

        public DeleteHolidaysCommandHandler(ICalendarRepository calendarRepository)
        {
            this.calendarRepository = calendarRepository;
        }

        public async Task Handle(DeleteHolidaysCommand request, CancellationToken cancellationToken)
        {
            var requestedHolidays = request.Holidays.Distinct().ToArray();

            var existingHolidays = await calendarRepository.GetAllAsync();

            var existingDates = existingHolidays
                .Select(x => x.HolidayDate)
                .ToHashSet();
            var missingDates = requestedHolidays
                .Where(x => !existingDates.Contains(x))
                .ToArray();

            if (missingDates.Length > 0)
            {
                throw new KeyNotFoundException(
                    $"Holidays not found: {string.Join(", ", missingDates)}");
            }

            await calendarRepository.RemoveHolidays(requestedHolidays);
        }
    }
}
