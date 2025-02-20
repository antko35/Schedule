using AutoMapper;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.Domain.Models;

namespace ScheduleService.Application.Mapping;

public class ScheduleRules : Profile
{
    public ScheduleRules()
    {
        CreateMap<SetGenerationRulesCommand, UserScheduleRules>()
            .AfterMap((src, dest) => ApplyRules(src, dest))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => IsValidMember(srcMember)));
    }

    private static void ApplyRules(SetGenerationRulesCommand src, UserScheduleRules dest)
    {
        if (src.EvenDOM == true || src.UnEvenDOM == true || src.UnEvenDOW == true ||
            src.EvenDOW == true || src.OnlyFirstShift == true || src.OnlySecondShift == true)
        {
            ResetFlags(dest);
        }

        if (src.EvenDOM == true) dest.EvenDOM = true;
        if (src.UnEvenDOM == true) dest.UnEvenDOM = true;
        if (src.UnEvenDOW == true) dest.UnEvenDOW = true;
        if (src.EvenDOW == true) dest.EvenDOW = true;
        if (src.OnlyFirstShift == true) dest.OnlyFirstShift = true;
        if (src.OnlySecondShift == true) dest.OnlySecondShift = true;
    }

    private static void ResetFlags(UserScheduleRules rules)
    {
        rules.OnlyFirstShift = false;
        rules.OnlySecondShift = false;
        rules.EvenDOM = false;
        rules.UnEvenDOM = false;
        rules.EvenDOW = false;
        rules.UnEvenDOW = false;
    }

    private static bool IsValidMember(object? srcMember) =>
        srcMember switch
        {
            null => false,
            TimeOnly time when time == default => false,
            float floatValue when floatValue == default => false,
            bool boolValue when boolValue == default => false,
            _ => true
        };
}