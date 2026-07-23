namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using MediatR;
    using MongoDB.Bson;
    using ScheduleService.Application.Extensions;
    using ScheduleService.Application.UseCases.Commands.Schedule;
    using ScheduleService.DataAccess.Repository;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class CreateWorkDayManuallyCommandHandler
        : IRequestHandler<CreateWorkDayManuallyCommand, WorkDay>
    {
        private readonly IUserRuleRepository userRuleRepository;
        private readonly IScheduleRepository scheduleRepository;

        private UserScheduleRules? userSchedueRules;

        public CreateWorkDayManuallyCommandHandler(
            IUserRuleRepository userRuleRepository,
            IScheduleRepository scheduleRepository)
        {
            this.userRuleRepository = userRuleRepository;
            this.scheduleRepository = scheduleRepository;
        }

        public async Task<WorkDay> Handle(CreateWorkDayManuallyCommand request, CancellationToken cancellationToken)
        {
            await GetUserScheduleRules(request);
            
            WorkDay newWorkDay = new WorkDay
            {
                Day = request.StartTime.Day,
                StartTime = DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc),
                EndTime = DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc),
            };

            var isAlreadyExist = await IsExist(request.StartTime.Day);

            if (isAlreadyExist)
            {
                await scheduleRepository.UpdateWorkDayAsync(userSchedueRules.ScheduleId, newWorkDay);
            }
            else
            {
                await scheduleRepository.AddWorkDayAsync(userSchedueRules.ScheduleId, newWorkDay);
            }

            return newWorkDay;
        }

        private async Task CorrectWeekSchedule(DateTime requestStartTime)
        {
            throw new NotImplementedException();
        }

        private async Task GetUserScheduleRules(CreateWorkDayManuallyCommand request)
        {
            string monthName = new DateTime(request.StartTime.Year, request.StartTime.Month, 1)
                .ToString("MMMM")
                .ToLower();

            this.userSchedueRules = await this.userRuleRepository
                .GetMonthScheduleRules(
                request.UserId,
                request.DepartmentId,
                monthName,
                request.StartTime.Year);

            if (this.userSchedueRules is null)
            {
                var scheduleId = ObjectId.GenerateNewId().ToString();

                var schedule = new Schedule { Id = scheduleId, MonthName = monthName };
                var rules = new UserScheduleRules
                {
                    UserId = request.UserId,
                    DepartmentId = request.DepartmentId,
                    Year = request.StartTime.Year,
                    MonthName = monthName,
                    ScheduleId = scheduleId,
                };

                await Task.WhenAll(
                    this.scheduleRepository.AddAsync(schedule),
                    this.userRuleRepository.AddAsync(rules));

                this.userSchedueRules = rules;
            }
        }

        private async Task<bool> IsExist(int day)
        {
            var dailySchedule = await scheduleRepository.GetWorkDayAsync(userSchedueRules.ScheduleId, day);

            if (dailySchedule == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
