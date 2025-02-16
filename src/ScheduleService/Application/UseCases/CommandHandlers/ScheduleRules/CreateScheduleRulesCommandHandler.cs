namespace ScheduleService.Application.UseCases.Commands.ScheduleRules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MediatR;
    using MongoDB.Bson;
    using ScheduleService.DataAccess.Repository;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public record CreateScheduleRulesCommandHandler : IRequestHandler<CreateScheduleRulesCommand, (Schedule Schedule, UserScheduleRules ScheduleRules)>
    {
        private readonly IUserRuleRepository userRuleRepository;
        private readonly IScheduleRepository scheduleRepository;
        public CreateScheduleRulesCommandHandler(IUserRuleRepository userRuleRepository, IScheduleRepository scheduleRepository)
        {
            this.userRuleRepository = userRuleRepository;
            this.scheduleRepository = scheduleRepository;
        }

        // auto generation after adding to user management service
        public async Task<(Schedule Schedule, UserScheduleRules ScheduleRules)> Handle(CreateScheduleRulesCommand request, CancellationToken cancellationToken)
        {
            var monthName = new DateOnly(request.Year, request.Month, 1)
                .ToString("MMMM")
                .ToLower();

            var scheduleId = ObjectId.GenerateNewId().ToString();
            var scheduleRules = new UserScheduleRules
            {
                UserId = request.UserId,
                DepartmentId = request.DepartmentId,
                Year = request.Year,
                MonthName = monthName,
                ScheduleId = scheduleId,
            };

            var schedule = new Schedule
            {
                Id = scheduleId,
                MonthName = monthName,
            };

            var task1 = scheduleRepository.AddAsync(schedule);
            var task2 = userRuleRepository.AddAsync(scheduleRules);

            await Task.WhenAll(task1, task2);

            return (schedule, scheduleRules);
        }
    }
}
