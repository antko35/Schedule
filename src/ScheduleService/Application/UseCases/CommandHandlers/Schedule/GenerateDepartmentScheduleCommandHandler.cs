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

            int daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);

            var currentDay = new DateTime(request.Year, request.Month, 1);
            var dayOfWeek = currentDay.DayOfWeek;

            officialHolidays = await calendarRepository.GetMonthHolidays(request.Month);

            transferDays = await calendarRepository.GetMonthTransferDays(request.Month);

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
                for (int day = 1; day <= daysInMonth; day++)
                {
                    workDay.Day = day;

                    currentDay = new DateTime(request.Year, request.Month, day);
                    dayOfWeek = currentDay.DayOfWeek;

                    var officialHoliday = IsHoliday(officialHolidays, day);
                    if (officialHoliday)
                    {
                        continue;
                    }

                    var isTransferDay = IsTransferDay(day);
                    if ((dayOfWeek == DayOfWeek.Sunday || dayOfWeek == DayOfWeek.Saturday) && !isTransferDay)
                    {
                        continue;
                    }

                    await CreateWorkDay(userRules, workDay, dayOfWeek, day);

                    if (isTransferDay)
                    {
                        var replacedDay = GetReplacedDay(day);
                        if (replacedDay?.HolidayDate.Month != request.Month)
                        {
                            continue;
                        }
                        else
                        {
                            await CreateWorkDay(userRules, workDay, replacedDay.DayOfWeek, replacedDay.HolidayDate.Day);
                        }
                    }
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
