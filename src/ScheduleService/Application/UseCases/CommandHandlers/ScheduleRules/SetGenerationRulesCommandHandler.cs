using System.Globalization;
using AutoMapper;
using MediatR;
using ScheduleService.Application.Extensions;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Models;

namespace ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules
{
    public class SetGenerationRulesCommandHandler : IRequestHandler<SetGenerationRulesCommand>
    {
        private readonly IUserRuleRepository userRuleRepository;
        private readonly IMapper mapper;

        public SetGenerationRulesCommandHandler(
            IUserRuleRepository userRuleRepository,
            IMapper mapper)
        {
            this.userRuleRepository = userRuleRepository;
            this.mapper = mapper;
        }

        public async Task Handle(SetGenerationRulesCommand request, CancellationToken cancellationToken)
        {
            var rules = await userRuleRepository.GetByIdAsync(request.ScheduleRulesId);

            rules.EnsureExists("Schedule rules not found");

            ApplyRules(request, rules);

            await userRuleRepository.UpdateAsync(rules);
        }

        private void ApplyRules(SetGenerationRulesCommand src, UserScheduleRules dest)
        {
            if (IsValidMember(src.HoursPerMonth)) dest.HoursPerMonth = src.HoursPerMonth.Value;
            if (IsValidMember(src.MaxHoursPerDay)) dest.MaxHoursPerDay = src.MaxHoursPerDay.Value;
            if (IsValidMember(src.StartWorkDayTime)) dest.StartWorkDayTime = src.StartWorkDayTime;

            if (src.OnlyFirstShift == true || src.OnlySecondShift == true || src.EvenDOW == true ||
                src.UnEvenDOW == true || src.EvenDOM == true || src.UnEvenDOM == true)
            {
                ResetFlags(dest);
            }

            if (src.OnlyFirstShift == true) dest.OnlyFirstShift = true;
            if (src.OnlySecondShift == true) dest.OnlySecondShift = true;
            if (src.EvenDOW == true) dest.EvenDOW = true;
            if (src.UnEvenDOW == true) dest.UnEvenDOW = true;
            if (src.EvenDOM == true) dest.EvenDOM = true;
            if (src.UnEvenDOM == true) dest.UnEvenDOM = true;
        }

        private void ResetFlags(UserScheduleRules rules)
        {
            rules.OnlyFirstShift = false;
            rules.OnlySecondShift = false;
            rules.EvenDOW = false;
            rules.UnEvenDOW = false;
            rules.EvenDOM = false;
            rules.UnEvenDOM = false;
        }

        private bool IsValidMember(object? srcMember) =>
            srcMember switch
            {
                null => false,
                TimeOnly time when time == default => false,
                float floatValue when floatValue == default => false,
                bool boolValue when boolValue == default => false,
                _ => true
            };
    }
}
