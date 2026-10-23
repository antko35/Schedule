using System.Globalization;
using AutoMapper;
using MediatR;
using ScheduleService.Application.Extensions;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Models;

namespace ScheduleService.Application.UseCases.CommandHandlers.ScheduleRules
{
    public class SetGenerationRulesCommandHandler : IRequestHandler<SetGenerationRulesCommand, UserScheduleRules>
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

        public async Task<UserScheduleRules> Handle(
            SetGenerationRulesCommand request,
            CancellationToken cancellationToken)
        {
            var rules = await userRuleRepository.GetByIdAsync(request.ScheduleRulesId);

            rules.EnsureExists("Schedule rules not found");

            ApplyRules(request, rules);

            await userRuleRepository.UpdateAsync(rules);

            return rules;
        }

        private void ApplyRules(SetGenerationRulesCommand src, UserScheduleRules dest)
        {
            if (src.HoursPerMonth.HasValue) dest.HoursPerMonth = src.HoursPerMonth.Value;
            if (src.MaxHoursPerDay.HasValue) dest.MaxHoursPerDay = src.MaxHoursPerDay.Value;
            if (src.StartWorkDayTime.HasValue) dest.StartWorkDayTime = src.StartWorkDayTime.Value;

            if (HasShiftPeriodicity(src))
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

        private bool HasShiftPeriodicity(SetGenerationRulesCommand rules) =>
            rules.OnlyFirstShift.HasValue ||
            rules.OnlySecondShift.HasValue ||
            rules.EvenDOW.HasValue ||
            rules.UnEvenDOW.HasValue ||
            rules.EvenDOM.HasValue ||
            rules.UnEvenDOM.HasValue;

        private void ResetFlags(UserScheduleRules rules)
        {
            rules.OnlyFirstShift = false;
            rules.OnlySecondShift = false;
            rules.EvenDOW = false;
            rules.UnEvenDOW = false;
            rules.EvenDOM = false;
            rules.UnEvenDOM = false;
        }
    }
}
