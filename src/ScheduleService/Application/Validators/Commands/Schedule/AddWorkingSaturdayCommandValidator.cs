using FluentValidation;
using ScheduleService.Application.UseCases.Commands.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ScheduleService.Application.Extensions.Validation;
using ScheduleService.Domain.Models;

namespace ScheduleService.Application.Validators.Commands.Schedule
{
    public class AddWorkingSaturdayCommandValidator
        : AbstractValidator<AddWorkingSaturdayCommand>
    {
        public AddWorkingSaturdayCommandValidator()
        {
            RuleFor(x => x.Saturday)
                .Must(BeSaturday)
                .WithMessage("Must be saturday");

            RuleForEach(workDay => workDay.WorkingDays)
                .ValidateWorkDayTimes();
        }

        private bool BeSaturday(WorkDay date)
        {
            return date.StartTime.Date.DayOfWeek == DayOfWeek.Saturday;
        }
    }
}
