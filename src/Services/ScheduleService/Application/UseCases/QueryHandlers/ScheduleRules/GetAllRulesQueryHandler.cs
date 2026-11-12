namespace ScheduleService.Application.UseCases.QueryHandlers.ScheduleRules;

using MediatR;
using ScheduleService.Application.Contracts;
using ScheduleService.Application.Mapping;
using ScheduleService.Application.UseCases.Queries.ScheduleRules;
using ScheduleService.Domain.Abstractions;

public sealed class GetAllRulesQueryHandler
    : IRequestHandler<GetAllRulesQuery, IEnumerable<ScheduleRulesDto>>
{
    private readonly IScheduleGenerationRuleRepository ruleRepository;
    private readonly IScheduleRepository scheduleRepository;

    public GetAllRulesQueryHandler(
        IScheduleGenerationRuleRepository ruleRepository,
        IScheduleRepository scheduleRepository)
    {
        this.ruleRepository = ruleRepository;
        this.scheduleRepository = scheduleRepository;
    }

    public async Task<IEnumerable<ScheduleRulesDto>> Handle(
        GetAllRulesQuery request,
        CancellationToken cancellationToken)
    {
        var rules = await ruleRepository.GetAllAsync(cancellationToken);
        var result = new List<ScheduleRulesDto>(rules.Count);

        foreach (var rule in rules)
        {
            var schedule = await scheduleRepository.GetAsync(
                rule.UserId,
                rule.DepartmentId,
                rule.Period,
                cancellationToken);
            result.Add(rule.ToDto(schedule?.Id ?? string.Empty));
        }

        return result;
    }
}
