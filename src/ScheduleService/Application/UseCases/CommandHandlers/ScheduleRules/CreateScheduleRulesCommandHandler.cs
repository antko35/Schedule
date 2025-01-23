using MediatR;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.UseCases.Commands.ScheduleRules
{
    public record CreateScheduleRulesCommandHandler : IRequestHandler<CreateScheduleRulesCommand>
    {
        private readonly IUserRuleRepository userRuleRepository;
        public CreateScheduleRulesCommandHandler(IUserRuleRepository userRuleRepository)
        {
            this.userRuleRepository = userRuleRepository;
        }

        // auto generation after adding to user management service
        public async Task Handle(CreateScheduleRulesCommand request, CancellationToken cancellationToken)
        {
            var scheduleRules = new UserScheduleRules
            {
                UserId = request.UserId,
                DepartmentId = request.DepartmentId,
                Month = request.Month,
            };

            await userRuleRepository.AddAsync(scheduleRules);
        }
    }
}
