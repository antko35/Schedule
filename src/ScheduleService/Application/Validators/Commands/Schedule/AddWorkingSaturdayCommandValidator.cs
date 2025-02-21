using FluentValidation;
using ScheduleService.Application.UseCases.Commands.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

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
        }

        private bool BeSaturday(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday;
        }
    }
}
