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
                if (src.EvenDOM.HasValue)
                {
                    dest.OnlyFirstShift = false;
                    dest.OnlySecondShift = false;
                    dest.UnEvenDOM = false;
                    dest.EvenDOW = false;
                    dest.UnEvenDOW = false;
                }

                if (src.UnEvenDOM.HasValue)
                {
                    dest.OnlyFirstShift = false;
                    dest.OnlySecondShift = false;
                    dest.EvenDOM = false;
                    dest.EvenDOW = false;
                    dest.UnEvenDOW = false;
                }

                if (src.UnEvenDOW.HasValue)
                {
                    dest.OnlyFirstShift = false;
                    dest.OnlySecondShift = false;
                    dest.EvenDOW = false;
                    dest.EvenDOM = false;
                    dest.UnEvenDOM = false;
                }

                if (src.EvenDOW.HasValue)
                {
                    dest.OnlyFirstShift = false;
                    dest.OnlySecondShift = false;
                    dest.UnEvenDOW = false;
                    dest.EvenDOM = false;
                    dest.UnEvenDOM = false;
                }

                if (src.OnlyFirstShift.HasValue)
                {
                    dest.OnlySecondShift = false;
                    dest.EvenDOW = false;
                    dest.UnEvenDOW = false;
                    dest.EvenDOM = false;
                    dest.UnEvenDOM = false;
                }

                if (src.OnlySecondShift.HasValue)
                {
                    dest.OnlyFirstShift = false;
                    dest.EvenDOW = false;
                    dest.UnEvenDOW = false;
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
