using MediatR;
using ScheduleService.Domain.Models;

namespace ScheduleService.Application.UseCases.Queries.ScheduleRules;

public record GetAllRulesQuery
    : IRequest<IEnumerable<UserScheduleRules>>;