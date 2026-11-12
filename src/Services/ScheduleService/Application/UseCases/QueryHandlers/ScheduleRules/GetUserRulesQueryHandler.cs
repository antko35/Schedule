namespace ScheduleService.Application.UseCases.QueryHandlers.ScheduleRules;

using MediatR;
using ScheduleService.Application.Contracts;
using ScheduleService.Application.Extensions;
using ScheduleService.Application.Mapping;
using ScheduleService.Application.UseCases.Queries.ScheduleRules;
using ScheduleService.Domain.Abstractions;

public sealed class GetUserRulesQueryHandler
    : IRequestHandler<GetUserRulesQuery, ScheduleRulesDto>
{
    private readonly IScheduleGenerationRuleRepository ruleRepository;
    private readonly IScheduleRepository scheduleRepository;

    public GetUserRulesQueryHandler(
        IScheduleGenerationRuleRepository ruleRepository,
        IScheduleRepository scheduleRepository)
    {
        this.ruleRepository = ruleRepository;
        this.scheduleRepository = scheduleRepository;
    }

    public async Task<ScheduleRulesDto> Handle(
        GetUserRulesQuery request,
        CancellationToken cancellationToken)
    {
        var period = new DateOnly(request.Year, request.Month, 1);
        var rule = await ruleRepository.GetAsync(
            request.UserId,
            request.DepartmentId,
            period,
            cancellationToken);
        rule.EnsureExists("Rules not found");

        var schedule = await scheduleRepository.GetAsync(
            request.UserId,
            request.DepartmentId,
            period,
            cancellationToken);

        return rule!.ToDto(schedule?.Id ?? string.Empty);
    }
}
