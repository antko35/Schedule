namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using MediatR;
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

            var isSaturday = IsSaturday(request.StartTime);

            WorkDay newWorkDay = new WorkDay
            {
                Day = request.StartTime.Day,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
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

            if (isSaturday)
            {
                await CorrectWeekSchedule(request.StartTime);
            }

            return newWorkDay;
        }

        private async Task CorrectWeekSchedule(DateTime requestStartTime)
        {
            throw new NotImplementedException();
        }

        private bool IsSaturday(DateTime startTime)
        {
            int dayOfWeek = (int)startTime.DayOfWeek;

            if (dayOfWeek == 6)
            {
                return true;
            }

            return false;
        }

        private async Task GetUserScheduleRules(CreateWorkDayManuallyCommand request)
        {
            string monthName = new DateTime(request.StartTime.Year, request.StartTime.Month, 1)
                .ToString("MMMM")
                .ToLower();

            userSchedueRules = await userRuleRepository
                .GetMonthScheduleRules(
                request.UserId,
                request.DepartmentId,
                monthName,
                request.StartTime.Year);

            userSchedueRules.EnsureExists($"Schedule rules not found, {monthName}");

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
