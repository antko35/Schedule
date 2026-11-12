namespace ScheduleService.Application.UseCases.Queries.Schedule
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Application.Contracts;

    public record GetDepartmentMonthScheduleQuery(
        string DepartmentId,
        int Year,
        int Month)
        : IRequest<List<ScheduleDto>>;
}
