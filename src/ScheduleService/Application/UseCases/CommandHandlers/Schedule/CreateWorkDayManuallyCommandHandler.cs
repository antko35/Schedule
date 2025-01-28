namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Application.UseCases.Commands.Schedule;
    using ScheduleService.DataAccess.Repository;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class CreateWorkDayManuallyCommandHandler
        : IRequestHandler<CreateWorkDayManuallyCommand, WorkDay>
    {
        private readonly IUserRuleRepository userRuleRepository;
        private readonly IScheduleRepository scheduleRepository;

        public CreateWorkDayManuallyCommandHandler(
            IUserRuleRepository userRuleRepository,
            IScheduleRepository scheduleRepository)
        {
            this.userRuleRepository = userRuleRepository;
            this.scheduleRepository = scheduleRepository;
        }

        public async Task<WorkDay> Handle(CreateWorkDayManuallyCommand request, CancellationToken cancellationToken)
        {
            string monthName = new DateTime(request.StartTime.Year, request.StartTime.Month, 1)
                .ToString("MMMM")
                .ToLower();

            var userSchedueRules = await userRuleRepository.GetMonthScheduleRules(request.UserId, request.DepartmentId, monthName)
                ?? throw new InvalidOperationException($"Schedule rules not found, {monthName}");

            var dailySchedule = await scheduleRepository.GetWorkDayAsync(userSchedueRules.ScheduleId, request.StartTime.Day);

            WorkDay newWorkDay = new WorkDay
            {
                Day = request.StartTime.Day,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
            };

            if (dailySchedule == null)
            {
                await scheduleRepository.AddWorkDayAsync(userSchedueRules.ScheduleId, newWorkDay);
            }
            else
            {
                await scheduleRepository.UpdateWorkDayAsync(userSchedueRules.ScheduleId, newWorkDay);
            }

            return newWorkDay;
        }
    }
}
