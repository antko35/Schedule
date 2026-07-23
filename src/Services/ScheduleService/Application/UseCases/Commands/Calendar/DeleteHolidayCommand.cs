using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.UseCases.Commands.Calendar
{
    public record DeleteHolidayCommand(DateOnly Holiday)
        : IRequest;
}
