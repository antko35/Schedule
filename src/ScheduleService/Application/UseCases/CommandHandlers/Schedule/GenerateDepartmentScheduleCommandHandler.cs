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

    public class GenerateDepartmentScheduleCommandHandler
        : IRequestHandler<GenerateDepartmentScheduleCommand>
    {
        private readonly IUserRuleRepository userRuleRepository;
        private readonly IScheduleRepository scheduleRepository;
        private readonly ICalendarRepository calendarRepository;

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
            string monthName = new DateTime(request.Year, request.Month, 1)
                .ToString("MMMM")
                .ToLower();

            var usersRules = await userRuleRepository.GetUsersRulesByDepartment(request.DepartmentId, monthName);

            int daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);

            var currentDay = new DateTime(request.Year, request.Month, 1);
            var dayOfWeek = currentDay.DayOfWeek;

            //var officialHoliday = await calendarRepository.GetMonthHolidays(request.Month);

            foreach (var userRules in usersRules)
            {
                var workDay = new AddWorkDayDto
                {
                   UserId = userRules.UserId,
                   DepartmentId = userRules.DepartmentId,
                   ScheduleId = userRules.ScheduleId,
                   Year = request.Year,
                   Month = request.Month,
                   MonthName = monthName,
                };
                for (int i = 1; i <= daysInMonth; i++)
                {
                    workDay.Day = i;

                    currentDay = new DateTime(request.Year, request.Month, i);
                    dayOfWeek = currentDay.DayOfWeek;

                    // проверка на гос выходной
                    if (dayOfWeek == DayOfWeek.Sunday || dayOfWeek == DayOfWeek.Saturday)
                    {
                        continue;
                    }

                    // if user works first shift on even days of week
                    if (userRules.EvenDOW)
                    {
                        if (dayOfWeek == DayOfWeek.Tuesday || dayOfWeek == DayOfWeek.Thursday)
                        {
                            await this.CreateAndAddWorkDay(workDay, userRules.FirstShift);
                        }

                        if (dayOfWeek == DayOfWeek.Monday || dayOfWeek == DayOfWeek.Wednesday || dayOfWeek == DayOfWeek.Friday)
                        {
                            await this.CreateAndAddWorkDay(workDay, userRules.SecondShift);
                        }
                    }

                    if (userRules.UnEvenDOW)
                    {
                        if (dayOfWeek == DayOfWeek.Monday || dayOfWeek == DayOfWeek.Wednesday || dayOfWeek == DayOfWeek.Friday)
                        {
                            await this.CreateAndAddWorkDay(workDay, userRules.FirstShift);
                        }

                        if (dayOfWeek == DayOfWeek.Tuesday || dayOfWeek == DayOfWeek.Thursday)
                        {
                            await this.CreateAndAddWorkDay(workDay, userRules.SecondShift);
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
                            await this.CreateAndAddWorkDay(workDay, userRules.SecondShift);
                        }
                    }

                    if (userRules.UnEvenDOM)
                    {
                        if (i % 2 != 0)
                        {
                            await this.CreateAndAddWorkDay(workDay, userRules.FirstShift);
                        }
                        else
                        {
                            await this.CreateAndAddWorkDay(workDay, userRules.SecondShift);
                        }
                    }
                }
            }
        }

        private async Task CreateAndAddWorkDay(AddWorkDayDto addWorkDay, bool firstShift)
        {
            var workDay = new WorkDay
            {
                Day = addWorkDay.Day,
                StartTime = firstShift
                                ? new DateTime(addWorkDay.Year, addWorkDay.Month, addWorkDay.Day, 8, 0, 0)
                                : new DateTime(addWorkDay.Year, addWorkDay.Month, addWorkDay.Day, 14, 30, 0),
                EndTime = firstShift
                                ? new DateTime(addWorkDay.Year, addWorkDay.Month, addWorkDay.Day, 14, 30, 0)
                                : new DateTime(addWorkDay.Year, addWorkDay.Month, addWorkDay.Day, 21, 0, 0),
            };

            var existingWorkDay = await scheduleRepository.GetWorkDayAsync(addWorkDay.ScheduleId, addWorkDay.Day);

            var isDayExist = existingWorkDay != null;
            if (isDayExist)
            {
                await scheduleRepository.UpdateWorkDayAsync(addWorkDay.ScheduleId, workDay);
            }
            else
            {
                await scheduleRepository.AddWorkDayAsync(addWorkDay.ScheduleId, workDay);
            }
        }
    }
}
