namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.VisualBasic;
    using ScheduleService.Application.DTOs;
    using ScheduleService.Application.UseCases.Commands.Schedule;
    using ScheduleService.DataAccess.Repository;
    using ScheduleService.Domain.Abstractions;
    using ScheduleService.Domain.Models;

    public class GenerateDepartmentScheduleCommandHandler : IRequestHandler<GenerateDepartmentScheduleCommand>
    {
        private readonly IUserRuleRepository userRuleRepository;

        public GenerateDepartmentScheduleCommandHandler(IUserRuleRepository userRuleRepository)
        {
            this.userRuleRepository = userRuleRepository;
        }

        public async Task Handle(GenerateDepartmentScheduleCommand request, CancellationToken cancellationToken)
        {
            string monthName = new DateTime(request.Year, request.Month, 1).ToString("MMMM", CultureInfo.CreateSpecificCulture("es"));

            var usersRules = await userRuleRepository.GetUsersRulesByDepartment(request.DepartmentId, monthName);

            int daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);

            DateTime firstDayOfMonth = new DateTime(request.Year, request.Month, 1);
            DayOfWeek dayOfWeek = firstDayOfMonth.DayOfWeek;

            foreach (var userRules in usersRules)
            {
                var workDay = new AddWorkDayDto
                {
                   UserId = userRules.UserId,
                   DepartmentId = userRules.DepartmentId,
                   Year = request.Year,
                   Month = request.Month,
                   MonthName = monthName,
                };
                for (int i = 1; i <= daysInMonth; i++)
                {
                    workDay.Day = i;

                    // проверка на гос выходной
                    if (dayOfWeek == DayOfWeek.Sunday || dayOfWeek == DayOfWeek.Saturday)
                    {
                        dayOfWeek++;
                        continue;
                    }

                    if (userRules.EvenDOW)
                    {
                        if (dayOfWeek == DayOfWeek.Tuesday || dayOfWeek == DayOfWeek.Thursday)
                        {
                            await this.CreateAndAddWorkDay(workDay, userRules.FirstShift);
                        }

                        if (dayOfWeek == DayOfWeek.Monday || dayOfWeek == DayOfWeek.Wednesday || dayOfWeek == DayOfWeek.Friday)
                        {
                            await this.CreateAndAddWorkDay(workDay, userRules.FirstShift);
                        }
                    }

                    if (userRules.EvenDOM)
                    {
                        if (i % 2 == 0)
                        {
                            await this.CreateAndAddWorkDay(workDay, userRules.FirstShift);
                        }
                        else
                        {
                            await this.CreateAndAddWorkDay(workDay, userRules.FirstShift);
                        }
                    }
                }
            }
        }

        private async Task CreateAndAddWorkDay(AddWorkDayDto addWorkDay, bool firstShift)
        {
            var workDay = new WorkDay
            {
                StartTime = firstShift
                                ? new DateTime(addWorkDay.Year, addWorkDay.Month, addWorkDay.Day, 8, 0, 0)
                                : new DateTime(addWorkDay.Year, addWorkDay.Month, addWorkDay.Day, 14, 30, 0),
                EndTime = firstShift
                                ? new DateTime(addWorkDay.Year, addWorkDay.Month, addWorkDay.Day, 14, 30, 0)
                                : new DateTime(addWorkDay.Year, addWorkDay.Month, addWorkDay.Day, 21, 0, 0),
            };

            await userRuleRepository.AddWorkDayAsync(addWorkDay.UserId, addWorkDay.DepartmentId, addWorkDay.MonthName, workDay);
        }
    }
}
