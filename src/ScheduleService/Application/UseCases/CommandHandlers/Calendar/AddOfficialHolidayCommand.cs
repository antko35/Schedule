using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.UseCases.CommandHandlers.Calendar
{
    public record AddOfficialHolidayCommand : IRequest
    {
        public DateOnly Holiday { get; set; }
        public DateOnly TransferDay { get; set; }
    }
}
