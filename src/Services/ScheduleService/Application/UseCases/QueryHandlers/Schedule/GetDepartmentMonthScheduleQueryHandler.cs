namespace ScheduleService.Application.UseCases.QueryHandlers.Schedule
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Application.UseCases.Queries.Schedule;
    using ScheduleService.DataAccess.Repository;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class GetDepartmentMonthScheduleQueryHandler
        : IRequestHandler<GetDepartmentMonthScheduleQuery, List<Schedule>>
    {
        private readonly IScheduleRepository scheduleRepository;
        private readonly IUserRuleRepository userRuleRepository;

        public GetDepartmentMonthScheduleQueryHandler(
            IScheduleRepository scheduleRepository,
            IUserRuleRepository userRuleRepository)
        {
            this.scheduleRepository = scheduleRepository;
            this.userRuleRepository = userRuleRepository;
        }

        public async Task<List<Schedule>> Handle(GetDepartmentMonthScheduleQuery request, CancellationToken cancellationToken)
        {
            var response = new List<Schedule>();

            var usersRules = await GetUsersRules(request);

            foreach (var userRule in usersRules)
            {
                var schedule = await scheduleRepository.GetMonthSchedule(userRule.ScheduleId);

                response.Add(schedule);
            }

            return response;
        }

        private async Task<IEnumerable<UserScheduleRules>> GetUsersRules(GetDepartmentMonthScheduleQuery request)
        {
            var monthName = new DateOnly(request.Year, request.Month, 1)
                .ToString("MMMM")
                .ToLower();

            var usersRules = await userRuleRepository.GetUsersRulesByDepartment(request.DepartmentId, monthName, request.Year);

            if (!usersRules.Any())
            {
                throw new InvalidOperationException("Schedule rules not found");
            }

            return usersRules;
        }
    }
}
