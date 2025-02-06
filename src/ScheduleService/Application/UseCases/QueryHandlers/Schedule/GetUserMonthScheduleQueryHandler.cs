namespace ScheduleService.Application.UseCases.QueryHandlers.Schedule
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Application.Extensions;
    using ScheduleService.Application.UseCases.Queries.Schedule;
    using ScheduleService.DataAccess.Repository;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class GetUserMonthScheduleQueryHandler
        : IRequestHandler<GetUserMonthScheduleQuery, Schedule>
    {
        private readonly IScheduleRepository scheduleRepository;
        private readonly IUserRuleRepository userRuleRepository;

        public GetUserMonthScheduleQueryHandler(
            IScheduleRepository scheduleRepository,
            IUserRuleRepository userRuleRepository)
        {
            this.scheduleRepository = scheduleRepository;
            this.userRuleRepository = userRuleRepository;
        }

        public async Task<Schedule> Handle(GetUserMonthScheduleQuery request, CancellationToken cancellationToken)
        {
            var monthName = new DateOnly(request.Year, request.Month, 1)
                .ToString("MMMM")
                .ToLower();

            var userRules = await userRuleRepository
                .GetMonthScheduleRules(request.UserId, request.DepartmentId, monthName, request.Year);

            userRules.EnsureExists("Invalid input");

            var schedule = await scheduleRepository.GetByIdAsync(userRules.ScheduleId);

            schedule.EnsureExists("Error while getting schedule");

            return schedule;
        }
    }
}
