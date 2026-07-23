using FluentValidation;
using ScheduleService.Application.Extensions.Validation;
using ScheduleService.Application.UseCases.Queries.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.Validators.Queris.Schedule
{
    public sealed class GetDepartmentMonthScheduleQueryValidator 
        : AbstractValidator<GetDepartmentMonthScheduleQuery>
    {
        public GetDepartmentMonthScheduleQueryValidator()
        {
            RuleFor(x => x.DepartmentId)
                .MustBeValidObjectId();

            RuleFor(x => x.Year)
                .InclusiveBetween(2000, 2100);

            RuleFor(x => x.Month)
                 .InclusiveBetween(1, 12);
        }
    }
}
