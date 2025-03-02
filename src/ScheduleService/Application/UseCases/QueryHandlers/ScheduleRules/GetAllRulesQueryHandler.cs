using MediatR;
using ScheduleService.Application.UseCases.Queries.ScheduleRules;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Models;

namespace ScheduleService.Application.UseCases.QueryHandlers.ScheduleRules;

public class GetAllRulesQueryHandler : IRequestHandler<GetAllRulesQuery, IEnumerable<UserScheduleRules>>
{
    private readonly IUserRuleRepository userRuleRepository;

    public GetAllRulesQueryHandler(IUserRuleRepository userRuleRepository)
    {
        this.userRuleRepository = userRuleRepository;
    }

    public async Task<IEnumerable<UserScheduleRules>> Handle(GetAllRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await userRuleRepository.GetAllAsync();

        return rules;
    }
}