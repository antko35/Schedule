using AutoMapper;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.Domain.Models;

namespace ScheduleService.Application.Mapping;

public class ScheduleRules : Profile
{
    public ScheduleRules()
    {
        CreateMap<SetGenerationRulesCommand, UserScheduleRules>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) =>
            {
                if (srcMember == null)
                    return false;

                if (srcMember is TimeOnly time && time == default(TimeOnly))
                    return false;

                if (srcMember is float floatValue && floatValue == default(float))
                    return false;

                if (srcMember is bool boolValue && boolValue == default(bool))
                    return false;

                return true;
            }));
    }
}