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
    using ScheduleService.Application.DTOs;
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

        private IEnumerable<UserScheduleRules>? usersRules;
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
            await GetUsersRulesForScheduleGeneration(request.Year, request.Month, request.DepartmentId);

            await GetHolidays(request.Year, request.Month);

            foreach (var userRules in usersRules)
            {
                await scheduleRepository.DeleteMonthSchedule(userRules.ScheduleId);

                var generatedDays = ScheduleGenerator.GenerateWorkDaysForUser(
                   userRules,
                   request.Year,
                   request.Month,
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

            this.usersRules = await userRuleRepository.GetUsersRulesByDepartment(departmentId, monthName);

            if (!usersRules.Any())
            {
                throw new InvalidOperationException("Schedule rules for this department not found");
            }
        }

        private async Task GetHolidays(int year, int month)
        {
            this.officialHolidays = await calendarRepository.GetMonthHolidays(year, month);

            this.transferDays = await calendarRepository.GetMonthTransferDays(year, month);
        }
    }
}
