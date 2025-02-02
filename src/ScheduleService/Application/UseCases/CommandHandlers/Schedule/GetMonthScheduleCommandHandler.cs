namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Application.UseCases.Commands.Schedule;
    using ScheduleService.DataAccess.Repository;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class GetMonthScheduleCommandHandler
        : IRequestHandler<GetMonthScheduleCommand>
    {
        private readonly IScheduleRepository scheduleRepository;
        private readonly IUserRuleRepository userRuleRepository;

        public GetMonthScheduleCommandHandler(
            IScheduleRepository scheduleRepository,
            IUserRuleRepository userRuleRepository)
        {
            this.scheduleRepository = scheduleRepository;
            this.userRuleRepository = userRuleRepository;
        }

        public async Task Handle(GetMonthScheduleCommand request, CancellationToken cancellationToken)
        {
            var monthName = new DateOnly(request.Year, request.Month, 1)
                .ToString("MMMM")
                .ToLower();

            var response = new List<Schedule>();

            var userRules = await userRuleRepository.GetUsersRulesByDepartment(request.DepartmentId, monthName)
                ?? throw new InvalidOperationException("Schedule rules not found");

            foreach (var userRule in userRules)
            {
                var schedule = await scheduleRepository.GetMonthSchedule(userRule.ScheduleId);

                response.Add(schedule);
            }

            //return response;
        }
    }
}
