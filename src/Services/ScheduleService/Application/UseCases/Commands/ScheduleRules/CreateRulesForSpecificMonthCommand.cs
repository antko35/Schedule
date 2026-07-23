using MediatR;

namespace ScheduleService.Application.UseCases.Commands.ScheduleRules;

public record CreateRulesForSpecificMonthCommand(
    string DepartmentId,
    int Year,
    int Month) : IRequest;
