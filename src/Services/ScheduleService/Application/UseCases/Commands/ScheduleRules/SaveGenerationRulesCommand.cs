using System.Text.Json.Serialization;
using MediatR;
using ScheduleService.Domain.Models;

namespace ScheduleService.Application.UseCases.Commands.ScheduleRules;

public record SaveGenerationRulesCommand(
    float HoursPerMonth,
    float MaxHoursPerDay,
    TimeOnly StartWorkDayTime,
    bool OnlyFirstShift,
    bool OnlySecondShift,
    bool EvenDOW,
    bool UnEvenDOW,
    bool EvenDOM,
    bool UnEvenDOM) : IRequest<UserScheduleRules>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    [JsonIgnore]
    public string DepartmentId { get; init; } = string.Empty;

    [JsonIgnore]
    public int Year { get; init; }

    [JsonIgnore]
    public int Month { get; init; }
}
