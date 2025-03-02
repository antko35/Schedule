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
    public sealed class DeleteWorkDayCommandValidator
        : AbstractValidator<DeleteWorkDayCommand>
    {
        public DeleteWorkDayCommandValidator()
        {
            RuleFor(x => x.UserId)
                .MustBeValidObjectId();

            RuleFor(x => x.DepartmentId)
                .MustBeValidObjectId();
        }
    }
}
