namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using ScheduleService.Application.UseCases.Commands.Schedule;
    using ScheduleService.DataAccess.Repository;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class GetUserMonthScheduleCommandHandler
        : IRequestHandler<GetUserMonthScheduleCommand, Schedule>
    {
        private readonly IScheduleRepository scheduleRepository;
        private readonly IUserRuleRepository userRuleRepository;

        public GetUserMonthScheduleCommandHandler(
            IScheduleRepository scheduleRepository,
            IUserRuleRepository userRuleRepository)
        {
            this.scheduleRepository = scheduleRepository;
            this.userRuleRepository = userRuleRepository;
        }

        public async Task<Schedule> Handle(GetUserMonthScheduleCommand request, CancellationToken cancellationToken)
        {
            var monthName = new DateOnly(request.year, request.month, 1)
                .ToString("MMMM")
                .ToLower();

            var userRules = await  userRuleRepository.GetMonthScheduleRules(request.UserId, request.departmentId, monthName, request.year);
            throw new NotImplementedException();
        }
    }
}
