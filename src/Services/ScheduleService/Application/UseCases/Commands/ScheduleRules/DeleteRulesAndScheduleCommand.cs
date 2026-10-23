using MediatR;

namespace ScheduleService.Application.UseCases.Commands.ScheduleRules;

public record DeleteRulesAndScheduleCommand(
    string UserId,
    string DepartmentId) : IRequest;
