namespace ScheduleService.Application.UseCases.CommandHandlers.Calendar;
using MediatR;
using ScheduleService.DataAccess.Database;
using ScheduleService.Application.UseCases.Commands.Calendar;
using ScheduleService.Domain.Abstractions;
public sealed class DeleteHolidaysCommandHandler(ICalendarRepository calendarRepository, ScheduleDbContext dbContext)
    : IRequestHandler<DeleteHolidaysCommand>
{
    public async Task Handle(DeleteHolidaysCommand request, CancellationToken cancellationToken)
    {
        var dates = request.Holidays.Distinct().ToHashSet();
        var holidays = await calendarRepository.GetAllAsync(cancellationToken);
        var missing = dates.Except(holidays.Select(item => item.HolidayDate)).ToArray();
        if (missing.Length > 0)
            throw new KeyNotFoundException($"Holidays not found: {string.Join(", ", missing)}");
        foreach (var holiday in holidays.Where(item => dates.Contains(item.HolidayDate)))
            calendarRepository.Remove(holiday);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
