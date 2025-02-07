using FluentValidation;
using ScheduleService.Application.UseCases.Queries.Calendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.Validators.Queris.Calendar
{
    public class GetYearHolidaysQueryValidator 
        : AbstractValidator<GetYearHolidaysQuery>
    {
        public GetYearHolidaysQueryValidator()
        {
            RuleFor(x => x.Year)
                .InclusiveBetween(2000, 2100);
        }
    }
}
