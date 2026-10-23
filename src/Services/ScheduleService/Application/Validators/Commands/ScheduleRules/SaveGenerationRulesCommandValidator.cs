using FluentValidation;
using ScheduleService.Application.Extensions.Validation;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;

namespace ScheduleService.Application.Validators.Commands.ScheduleRules;

public sealed class SaveGenerationRulesCommandValidator
    : AbstractValidator<SaveGenerationRulesCommand>
{
    public SaveGenerationRulesCommandValidator()
    {
        RuleFor(x => x.UserId)
            .MustBeValidObjectId();

        RuleFor(x => x.DepartmentId)
            .MustBeValidObjectId();

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100);

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12);

        RuleFor(x => x.HoursPerMonth)
            .GreaterThan(0);

        RuleFor(x => x.MaxHoursPerDay)
            .GreaterThan(0);

        RuleFor(x => x)
            .Must(x => x.HoursPerMonth / x.MaxHoursPerDay is >= 15 and <= 23)
            .WithMessage("Invalid hours per month or work day length.");

        RuleFor(x => x)
            .Must(x => new[]
            {
                x.OnlyFirstShift,
                x.OnlySecondShift,
                x.EvenDOW,
                x.UnEvenDOW,
                x.EvenDOM,
                x.UnEvenDOM,
            }.Count(value => value) <= 1)
            .WithMessage("Only one schedule periodicity can be selected.");
    }
}
