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
            var year = DateTime.Now.Year;
            var monthName = DateTime.Now
                .ToString("MMMM")
                .ToLower();

            var scheduleId = ObjectId.GenerateNewId().ToString();
            var scheduleRules = new UserScheduleRules
            {
                UserId = request.UserId,
                DepartmentId = request.DepartmentId,
                Year = year,
                MonthName = monthName,
                ScheduleId = scheduleId,
            };

            var schedule = new Schedule
            {
                Id = scheduleId,
                MonthName = monthName,
            };

            var addScheduleTask = scheduleRepository.AddAsync(schedule);
            var addScheduleRulesTask = userRuleRepository.AddAsync(scheduleRules);

            await Task.WhenAll(addScheduleTask, addScheduleRulesTask);

            return (schedule, scheduleRules);
        }
    }
}
