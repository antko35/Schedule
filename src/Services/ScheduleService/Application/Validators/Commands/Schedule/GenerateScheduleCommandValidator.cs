namespace ScheduleService.Application.Validators.Commands.Schedule
{
    using FluentValidation;
    using Extensions.Validation;
    using ScheduleService.Application.UseCases.Commands.Schedule;

    public sealed class GenerateScheduleCommandValidator
        : AbstractValidator<GenerateScheduleCommand>
    {
        public GenerateScheduleCommandValidator()
        {
            RuleFor(x => x.UserId)
                .MustBeValidObjectId();

            RuleFor(x => x.DepartmentId)
                .MustBeValidObjectId();

            RuleFor(x => x.Year)
                .InclusiveBetween(2000, 2100);

            RuleFor(x => x.Month)
                .InclusiveBetween(1, 12);
        }
    }
}
