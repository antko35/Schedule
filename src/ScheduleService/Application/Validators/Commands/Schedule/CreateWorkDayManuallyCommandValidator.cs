using FluentValidation;
using ScheduleService.Application.Extensions.Validation;
using ScheduleService.Application.UseCases.Commands.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.Validators.Commands.Schedule
{
    public sealed class CreateWorkDayManuallyCommandValidator
        : AbstractValidator<CreateWorkDayManuallyCommand>
    {
        public CreateWorkDayManuallyCommandValidator()
        {
            RuleFor(x => x.UserId)
                .MustBeValidObjectId();

            RuleFor(x => x.DepartmentId)
                .MustBeValidObjectId();

            RuleFor(workDay => workDay.EndTime)
                .GreaterThanOrEqualTo(workDay => workDay.StartTime)
                .WithMessage("EndTime must be greater than or equal to StartTime.")
                .Must((workDay, endTime) => endTime.Date == workDay.StartTime.Date)
                .WithMessage("StartTime and EndTime must be on the same day.");
        }
    }
}
