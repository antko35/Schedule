using MediatR;
using ScheduleService.Application.Contracts;

namespace ScheduleService.Application.UseCases.Queries.ScheduleRules;

public record GetAllRulesQuery
    : IRequest<IEnumerable<ScheduleRulesDto>>;
