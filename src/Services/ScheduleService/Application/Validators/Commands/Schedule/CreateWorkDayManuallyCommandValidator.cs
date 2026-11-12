using FluentValidation;
using ScheduleService.Application.Extensions.Validation;
using ScheduleService.Application.UseCases.Commands.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScheduleService.Domain.Models;

namespace ScheduleService.Application.Validators.Commands.Schedule
{
    public sealed class CreateWorkDayManuallyCommandValidator
        : AbstractValidator<CreateWorkDayManuallyCommand>
    {
        public CreateWorkDayManuallyCommandValidator()
        {
            RuleFor(x => x.UserId)
                .MustBeValidEntityId();

            RuleFor(x => x.DepartmentId)
                .MustBeValidEntityId();

            RuleFor(workDay => workDay.EndTime)
                .GreaterThan(workDay => workDay.StartTime)
                .WithMessage("EndTime must be greater than StartTime.")
                .Must((workDay, endTime) => endTime.Date == workDay.StartTime.Date)
                .WithMessage("StartTime and EndTime must be on the same day.");
        }
    }
}
