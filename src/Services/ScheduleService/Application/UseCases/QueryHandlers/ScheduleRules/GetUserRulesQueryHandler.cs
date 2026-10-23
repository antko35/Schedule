using MediatR;
using ScheduleService.Application.Extensions;
using ScheduleService.Application.UseCases.Queries.ScheduleRules;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.UseCases.QueryHandlers.ScheduleRules
{
    public class GetUserRulesQueryHandler
        : IRequestHandler<GetUserRulesQuery, UserScheduleRules>
    {
        private readonly IUserRuleRepository userRuleRepository;

        public GetUserRulesQueryHandler(IUserRuleRepository userRuleRepository)
        {
            this.userRuleRepository = userRuleRepository;
        }

        public async Task<UserScheduleRules> Handle(GetUserRulesQuery request, CancellationToken cancellationToken)
        {
            var monthName = new DateTime(request.Year, request.Month, 1)
                .ToString("MMMM")
                .ToLower();

            var response = await userRuleRepository
                .GetMonthScheduleRules(
                request.UserId,
                request.DepartmentId,
                monthName,
                request.Year);

            response.EnsureExists("Rules not found");

            return response;
        }
    }
}
