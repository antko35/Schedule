using AutoMapper;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;

namespace ScheduleService.Application.Mapping;

public class ScheduleRules : Profile
{
    public ScheduleRules()
    {
        CreateMap<SetGenerationRulesCommand, ScheduleRules>();
    }
}