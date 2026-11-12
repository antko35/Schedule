using FluentValidation;
using ScheduleService.Application.Contracts;

namespace ScheduleService.Application.Extensions.Validation;

public static class WorkDayValidationExtensions
{
    public static IRuleBuilderOptions<T, WorkDayDto> ValidateWorkDayTimes<T>(this IRuleBuilder<T, WorkDayDto> ruleBuilder)
    {
        return ruleBuilder
            .Must(workDay => workDay.EndTime > workDay.StartTime)
            .WithMessage("EndTime must be greater than StartTime.")
            .Must(workDay => workDay.EndTime.Date == workDay.StartTime.Date)
            .WithMessage("StartTime and EndTime must be on the same day.");
    }
}
