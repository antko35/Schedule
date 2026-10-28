namespace ScheduleService.Application.UseCases.CommandHandlers.Calendar;

using MediatR;
using ScheduleService.DataAccess.Database;
using ScheduleService.Application.UseCases.Commands.Calendar;
using ScheduleService.Domain.Abstractions;

public sealed class DeleteHolidayCommandHandler : IRequestHandler<DeleteHolidayCommand>
{
    private readonly ICalendarRepository calendarRepository;
    private readonly ScheduleDbContext dbContext;

    public DeleteHolidayCommandHandler(
        ICalendarRepository calendarRepository,
        ScheduleDbContext dbContext)
    {
        this.calendarRepository = calendarRepository;
        this.dbContext = dbContext;
    }

    public async Task Handle(DeleteHolidayCommand request, CancellationToken cancellationToken)
    {
        var period = new DateOnly(request.Holiday.Year, request.Holiday.Month, 1);
        var holidays = await calendarRepository.GetMonthHolidaysAsync(period, cancellationToken);
        var holiday = holidays.FirstOrDefault(item => item.HolidayDate == request.Holiday)
            ?? throw new KeyNotFoundException("Day not found");

        calendarRepository.Remove(holiday);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
