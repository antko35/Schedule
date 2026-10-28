namespace ScheduleService.Application.UseCases.CommandHandlers.Calendar;

using MediatR;
using ScheduleService.DataAccess.Database;
using ScheduleService.Application.Contracts;
using ScheduleService.Application.Mapping;
using ScheduleService.Application.UseCases.Commands.Calendar;
using ScheduleService.Domain.Abstractions;
using ScheduleService.Domain.Models;

public sealed class AddOfficialHolidayCommandHandler
    : IRequestHandler<AddOfficialHolidayCommand, CalendarDto>
{
    private readonly ICalendarRepository calendarRepository;
    private readonly ScheduleDbContext dbContext;

    public AddOfficialHolidayCommandHandler(
        ICalendarRepository calendarRepository,
        ScheduleDbContext dbContext)
    {
        this.calendarRepository = calendarRepository;
        this.dbContext = dbContext;
    }

    public async Task<CalendarDto> Handle(
        AddOfficialHolidayCommand request,
        CancellationToken cancellationToken)
    {
        var period = new DateOnly(request.Holiday.Year, request.Holiday.Month, 1);
        var holidays = await calendarRepository.GetMonthHolidaysAsync(period, cancellationToken);
        if (holidays.Any(item => item.HolidayDate == request.Holiday))
        {
            throw new InvalidOperationException($"Holiday {request.Holiday} already exist");
        }

        DateOnly? transferDate = request.TransferDay == DateOnly.MinValue ? null : request.TransferDay;
        var calendarException = new CalendarException(
            Guid.NewGuid().ToString("N"),
            request.Holiday,
            transferDate);

        await calendarRepository.AddAsync(calendarException, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return calendarException.ToDto();
    }
}
