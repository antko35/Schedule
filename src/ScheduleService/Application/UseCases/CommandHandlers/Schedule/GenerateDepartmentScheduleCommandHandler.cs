namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;
    using global::Application.Services;
    using MediatR;
    using Microsoft.VisualBasic;
    using ScheduleService.Application.UseCases.Commands.Schedule;
    using ScheduleService.DataAccess.Repository;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class GenerateDepartmentScheduleCommandHandler
        : IRequestHandler<GenerateDepartmentScheduleCommand>
    {
        private readonly IUserRuleRepository userRuleRepository;
        private readonly IScheduleRepository scheduleRepository;
        private readonly ICalendarRepository calendarRepository;

        private IEnumerable<UserScheduleRules> usersRules = new List<UserScheduleRules>();
        private List<Calendar?> officialHolidays = new List<Calendar?>();
        private List<Calendar?> transferDays = new List<Calendar?>();

        public GenerateDepartmentScheduleCommandHandler(
            IUserRuleRepository userRuleRepository,
            IScheduleRepository scheduleRepository,
            ICalendarRepository calendarRepository)
        {
            this.userRuleRepository = userRuleRepository;
            this.scheduleRepository = scheduleRepository;
            this.calendarRepository = calendarRepository;
        }

        public async Task Handle(GenerateDepartmentScheduleCommand request, CancellationToken cancellationToken)
        {
            var task1 = GetUsersRulesForScheduleGeneration(request.Year, request.Month, request.DepartmentId);
            var task2 = GetHolidays(request.Year, request.Month);

            await Task.WhenAll(task1, task2);

            await GenerateAndAdd(request.Year, request.Month);
        }

        private async Task GenerateAndAdd(int year, int month)
        {
            foreach (var userRules in usersRules)
            {
                await scheduleRepository.DeleteMonthSchedule(userRules.ScheduleId);

                var generatedDays = ScheduleGenerator.GenerateWorkDaysForUser(
                    userRules,
                    year,
                    month,
                    officialHolidays,
                    transferDays);

                foreach (var workDay in generatedDays)
                {
                    await scheduleRepository.AddWorkDayAsync(userRules.ScheduleId, workDay);
                }
            }
        }

        private async Task GetUsersRulesForScheduleGeneration(int year, int month, string departmentId)
        {
            string monthName = new DateTime(year, month, 1)
                .ToString("MMMM")
                .ToLower();

            this.usersRules = await userRuleRepository.GetUsersRulesByDepartment(departmentId, monthName, year);

            if (!usersRules.Any())
            {
                throw new InvalidOperationException("Schedule rules for this department not found");
            }
        }

        private async Task GetHolidays(int year, int month)
        {
            var task1 = calendarRepository.GetMonthHolidays(year, month);
            var task2 = calendarRepository.GetMonthTransferDays(year, month);

            await Task.WhenAll(task1, task2);

            this.officialHolidays = task1.Result;
            this.transferDays = task2.Result;
        }
    }
}
