namespace ScheduleService.Application.UseCases.Commands.ScheduleRules;

using MediatR;
using Domain.Models;

public record CreateScheduleRulesCommand(
    string UserId,
    string DepartmentId) : IRequest<(Schedule Schedule, UserScheduleRules ScheduleRules)>;