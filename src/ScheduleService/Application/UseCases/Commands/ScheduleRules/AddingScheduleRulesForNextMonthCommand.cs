using MediatR;

namespace ScheduleService.Application.UseCases.Commands.ScheduleRules;

public record AddingScheduleRulesForNextMonthCommand : IRequest
{ }