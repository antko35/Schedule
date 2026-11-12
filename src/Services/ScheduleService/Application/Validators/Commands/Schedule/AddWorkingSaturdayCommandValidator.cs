using FluentValidation;
using ScheduleService.Application.UseCases.Commands.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ScheduleService.Application.Extensions.Validation;
using ScheduleService.Application.Contracts;

namespace ScheduleService.Application.Validators.Commands.Schedule
{
    public class AddWorkingSaturdayCommandValidator
        : AbstractValidator<AddWorkingSaturdayCommand>
    {
        public AddWorkingSaturdayCommandValidator()
        {
            RuleFor(x => x.ScheduleId)
                .MustBeValidEntityId();

            RuleFor(x => x.Saturday)
                .Must(BeSaturday)
                .WithMessage("Must be saturday");

            RuleFor(x => x.Saturday)
                .ValidateWorkDayTimes();

            RuleForEach(workDay => workDay.WorkingDays)
                .ValidateWorkDayTimes();

            RuleFor(x => x.WorkingDays)
                .Must(workDays => workDays
                    .Select(workDay => workDay.StartTime.Date)
                    .Distinct()
                    .Count() == workDays.Count)
                .WithMessage("WorkingDays must contain distinct dates.");
        }

        private bool BeSaturday(WorkDayDto date)
        {
            return date.StartTime.Date.DayOfWeek == DayOfWeek.Saturday;
        }
    }
}
