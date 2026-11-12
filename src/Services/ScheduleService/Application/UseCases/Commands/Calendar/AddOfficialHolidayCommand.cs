namespace ScheduleService.Application.UseCases.Commands.Calendar
{
    using System;
    using Contracts;
    using MediatR;

    public record AddOfficialHolidayCommand(DateOnly Holiday, DateOnly TransferDay)
        : IRequest<CalendarDto>;
}
