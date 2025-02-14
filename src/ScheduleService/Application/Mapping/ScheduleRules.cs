using AutoMapper;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.Domain.Models;

namespace ScheduleService.Application.Mapping;

public class ScheduleRules : Profile
{
    public ScheduleRules()
    {
        CreateMap<SetGenerationRulesCommand, UserScheduleRules>()
            .AfterMap((src, dest) =>
            {
                if (src.EvenDOM.HasValue || src.UnEvenDOM.HasValue)
                {
                    dest.EvenDOW = false;
                    dest.UnEvenDOW = false;
                }

                if (src.EvenDOW.HasValue || src.UnEvenDOW.HasValue)
                {
                    dest.EvenDOM = false;
                    dest.UnEvenDOM = false;
                }
            })
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
