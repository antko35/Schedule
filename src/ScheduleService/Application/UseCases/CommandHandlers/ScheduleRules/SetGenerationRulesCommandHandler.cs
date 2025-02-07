using MediatR;
using ScheduleService.Application.Extensions;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules
{
    public class SetGenerationRulesCommandHandler : IRequestHandler<SetGenerationRulesCommand>
    {
        private readonly IUserRuleRepository userRuleRepository;

        public SetGenerationRulesCommandHandler(IUserRuleRepository userRuleRepository)
        {
            this.userRuleRepository = userRuleRepository;
        }

        public async Task Handle(SetGenerationRulesCommand request, CancellationToken cancellationToken)
        {
            var rules = await userRuleRepository.GetByIdAsync(request.Id);

            rules.EnsureExists("Schedule rules not found");

            throw new NotImplementedException();
        }
    }
}
