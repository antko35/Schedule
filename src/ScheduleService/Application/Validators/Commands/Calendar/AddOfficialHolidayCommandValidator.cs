using FluentValidation;
using ScheduleService.Application.UseCases.Commands.Calendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.Validators.Commands.Calendar
{
    public sealed class AddOfficialHolidayCommandValidator
        : AbstractValidator<AddOfficialHolidayCommand>
    {
        public AddOfficialHolidayCommandValidator()
        {
            RuleFor(x => x.Holiday)
                .NotEmpty()
                .WithMessage("Holiday date is required.");

            RuleFor(x => x.TransferDay)
                .Must((command, transferDay) =>
                transferDay == DateOnly.MinValue ||
                transferDay.Year == command.Holiday.Year)
                .WithMessage("TransferDay must be in the same year as Holiday or be empty");

            RuleFor(x => x.TransferDay)
                .NotEqual(x => x.Holiday)
                .WithMessage("TransferDay cant be the same day as holiday");
        }
    }
}
