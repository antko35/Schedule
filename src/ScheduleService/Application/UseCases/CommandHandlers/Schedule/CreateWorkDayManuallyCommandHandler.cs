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

    public class CreateWorkDayManuallyCommandHandler : IRequestHandler<CreateWorkDayManuallyCommand>
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

        public async Task Handle(CreateWorkDayManuallyCommand request, CancellationToken cancellationToken)
        {
            string monthName = new DateTime(request.StartTime.Year, request.StartTime.Month, 1).ToString("MMMM")
                .ToLower();

            var userSchedueRules = await userRuleRepository.GetMonthScheduleRules(request.UserId, request.DepartmentId, monthName)
                ?? throw new InvalidOperationException($"Schedile rules not found, {monthName}");

            WorkDay workDay = await scheduleRepository.GetWorkDayAsync(userSchedueRules.ScheduleId, request.StartTime.Day);

            var newWorkDay = new WorkDay
            {
                StartTime = request.StartTime,
                EndTime = request.EndTime,
            };

            if (workDay != null)
            {
                await userRuleRepository.UpdateWorkDayAsync(request.UserId, request.DepartmentId, monthName, newWorkDay);
            }
            else
            {
                await scheduleRepository.AddWorkDayAsync(userSchedueRules.ScheduleId, newWorkDay);
            }
        }
    }
}
