namespace ScheduleService.Application.Validators.Commands.Calendar
{
    using FluentValidation;
    using ScheduleService.Application.UseCases.Commands.Calendar;

    public sealed class DeleteHolidaysCommandValidator
        : AbstractValidator<DeleteHolidaysCommand>
    {
        public DeleteHolidaysCommandValidator()
        {
            RuleFor(x => x.Holidays)
                .NotEmpty()
                .WithMessage("At least one holiday date is required.");

            RuleForEach(x => x.Holidays)
                .NotEmpty()
                .WithMessage("Holiday date is required.");
        }
    }
}
