using MediatR;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using ScheduleService.Application.Extensions;
using ScheduleService.Domain.Models;

namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule
{
    public class AddWorkingSaturdayCommandHandler
        : IRequestHandler<AddWorkingSaturdayCommand>
    {
        private readonly IScheduleRepository scheduleRepository;

        public AddWorkingSaturdayCommandHandler(IScheduleRepository scheduleRepository)
        {
            this.scheduleRepository = scheduleRepository;
        }

        public async Task Handle(AddWorkingSaturdayCommand request, CancellationToken cancellationToken)
        {
            await IsScheduleExist(request.ScheduleId);

            await IsSatturdayAlreadyExist(request.ScheduleId, request.Saturday);

            await ValidateNewWorkDays(request);

            await AddSaturday(request.Saturday, request.ScheduleId);

            await UpdateWorkDaysInMonth(request);
        }

        private async Task IsSatturdayAlreadyExist(string scheduleId, WorkDay saturday)
        {
            var day = await scheduleRepository.GetWorkDayAsync(scheduleId, saturday.Day);
            if (day != null)
            {
                throw new InvalidOperationException("Saturday already exist");
            }
        }

        private async Task ValidateNewWorkDays(AddWorkingSaturdayCommand request)
        {
            TimeSpan newTotalTime = TimeSpan.Zero;
            TimeSpan oldTotalTime = TimeSpan.Zero;

            CheckDaysCount(request.WorkingDays);

            foreach (var workDay in request.WorkingDays)
            {
                var day = await scheduleRepository.GetWorkDayAsync(request.ScheduleId, workDay.Day);
                day.EnsureExists($"No work found for this day({workDay})");

                var dayLength = workDay.EndTime.Subtract(workDay.StartTime);
                if (dayLength.Hours < 4)
                {
                    throw new InvalidOperationException("Minimum work day length is 4 hours");
                }

                newTotalTime += dayLength;
                oldTotalTime += day.WorkDays.First().EndTime.Subtract(day.WorkDays.First().StartTime);
            }

            IsTimeEqual(newTotalTime, oldTotalTime, request.Saturday);
        }

        private void IsTimeEqual(TimeSpan newTotalTime, TimeSpan oldTotalTime, WorkDay saturday )
        {
            var timeDiff = oldTotalTime - newTotalTime;
            var saturdayLength = saturday.EndTime.Subtract(saturday.StartTime);
            if (timeDiff != saturdayLength)
            {
                throw new InvalidOperationException($"Invalid time difference {timeDiff}");
            }
        }

        private void CheckDaysCount(List<WorkDay> workDays)
        {
            var count = workDays.Count;
            if (count != 5)
            {
                throw new InvalidOperationException("Should be 5 updated days");
            }
        }

        private async Task IsScheduleExist(string requestScheduleId)
        {
            var schedule = await scheduleRepository.GetByIdAsync(requestScheduleId);

            schedule.EnsureExists("Schedule not found");
        }

        private async Task UpdateWorkDaysInMonth(AddWorkingSaturdayCommand request)
        {
            await scheduleRepository.UpdateWorkDaysAsync(request.ScheduleId, request.WorkingDays);
        }

        private async Task AddSaturday(WorkDay saturday, string scheduleId)
        {
            await scheduleRepository.AddWorkDayAsync(scheduleId, saturday);
        }
    }
}
