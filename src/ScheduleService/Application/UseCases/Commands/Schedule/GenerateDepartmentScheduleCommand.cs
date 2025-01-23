using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.UseCases.Commands.Schedule
{
    public class GenerateDepartmentScheduleCommand
        : IRequest
    {
        required public string DepartmentId { get; set; }

        required public int Year { get; set; }

        required public int Month { get; set; }
    }
}
