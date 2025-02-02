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
            string monthName = new DateTime(request.Year, request.Month, 1)
                .ToString("MMMM")
                .ToLower();

            usersRules = await userRuleRepository.GetUsersRulesByDepartment(request.DepartmentId, monthName);

            if (!usersRules.Any())
            {
                throw new InvalidOperationException("Schedule rules for this department not found");
            }

            this.officialHolidays = await calendarRepository.GetMonthHolidays(request.Month);

            this.transferDays = await calendarRepository.GetMonthTransferDays(request.Month);

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

        private Calendar? GetReplacedDay(int i)
        {
            var day = transferDays.FirstOrDefault(x => x.HolidayDayOfMonth == i);

            return day;
        }

        private bool IsHoliday(IEnumerable<Calendar> holidays, int day)
        {
            return holidays.Any(x => x.HolidayDayOfMonth == day);
        }

        private bool IsTransferDay(int i)
        {
            return transferDays.Any(x => x?.TransferDate?.Day == i);
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

        private async Task CreateWorkDay(UserScheduleRules userRules, AddWorkDayDto workDay, DayOfWeek dayOfWeek, int dayOfMonth)
        {
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
                if (dayOfMonth % 2 == 0)
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
                if (dayOfMonth % 2 != 0)
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
