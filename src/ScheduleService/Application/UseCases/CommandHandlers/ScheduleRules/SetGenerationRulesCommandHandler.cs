using AutoMapper;
using MediatR;
using ScheduleService.Application.Extensions;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.DataAccess.Repository;

namespace ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules
{
    public class SetGenerationRulesCommandHandler : IRequestHandler<SetGenerationRulesCommand>
    {
        private readonly IUserRuleRepository userRuleRepository;
        private readonly IMapper mapper;

        public SetGenerationRulesCommandHandler(
            IUserRuleRepository userRuleRepository,
            IMapper mapper)
        {
            this.userRuleRepository = userRuleRepository;
            this.mapper = mapper;
        }

        public async Task Handle(SetGenerationRulesCommand request, CancellationToken cancellationToken)
        {
            var rules = await userRuleRepository.GetByIdAsync(request.ScheduleRulesId);

            rules.EnsureExists("Schedule rules not found");

            mapper.Map(request, rules);

            await userRuleRepository.UpdateAsync(rules);
        }
    }
}
