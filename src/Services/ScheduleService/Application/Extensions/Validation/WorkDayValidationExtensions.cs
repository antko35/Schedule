using FluentValidation;
using ScheduleService.Domain.Models;

namespace ScheduleService.Application.Extensions.Validation;

public static class WorkDayValidationExtensions
{
    public static IRuleBuilderOptions<T, WorkDay> ValidateWorkDayTimes<T>(this IRuleBuilder<T, WorkDay> ruleBuilder)
    {
        return ruleBuilder
            .Must(workDay => workDay.EndTime >= workDay.StartTime)
            .WithMessage("EndTime must be greater than or equal to StartTime.")
            .Must(workDay => workDay.EndTime.Date == workDay.StartTime.Date)
            .WithMessage("StartTime and EndTime must be on the same day.");
    }
}