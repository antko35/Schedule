using ScheduleService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public static class ScheduleGenerator
    {
        public static List<WorkDay> GenerateWorkDaysForUser(
            UserScheduleRules userRules,
            int year,
            int month,
            IEnumerable<Calendar> officialHolidays,
            IEnumerable<Calendar> transferDays)
        {
            var generatedDays = new List<WorkDay>();
            int daysInMonth = DateTime.DaysInMonth(year, month);

            for (int day = 1; day <= daysInMonth; day++)
            {
                var currentDay = new DateTime(year, month, day);
                DayOfWeek dayOfWeek = currentDay.DayOfWeek;

                // skip holidays
                if (officialHolidays.Any(x => x.HolidayDayOfMonth == day))
                {
                    continue;
                }

                // chek transfer date
                bool isTransferDay = transferDays.Any(x => x?.TransferDate?.Day == day);
                if ((dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday) && !isTransferDay)
                {
                    continue;
                }

                if (isTransferDay)
                {
                    var replacedDay = transferDays.FirstOrDefault(x => x?.TransferDate?.Day == day);
                    if (replacedDay == null || replacedDay.HolidayDate.Month != month)
                    {
                        continue;
                    }
                    else
                    {
                        dayOfWeek = replacedDay.HolidayDate.DayOfWeek;
                    }
                }

                if (userRules.EvenDOW)
                {
                    if (dayOfWeek == DayOfWeek.Tuesday || dayOfWeek == DayOfWeek.Thursday)
                    {
                        generatedDays.Add(CreateWorkDayDto(userRules, year, month, day, firstShift: true));
                    }

                    if (dayOfWeek == DayOfWeek.Monday || dayOfWeek == DayOfWeek.Wednesday || dayOfWeek == DayOfWeek.Friday)
                    {
                        generatedDays.Add(CreateWorkDayDto(userRules, year, month, day, firstShift: false));
                    }
                }

                if (userRules.UnEvenDOW)
                {
                    if (dayOfWeek == DayOfWeek.Monday || dayOfWeek == DayOfWeek.Wednesday || dayOfWeek == DayOfWeek.Friday)
                    {
                        generatedDays.Add(CreateWorkDayDto(userRules, year, month, day, firstShift: true));
                    }

                    if (dayOfWeek == DayOfWeek.Tuesday || dayOfWeek == DayOfWeek.Thursday)
                    {
                        generatedDays.Add(CreateWorkDayDto(userRules, year, month, day, firstShift: false));
                    }
                }

                if (userRules.EvenDOM)
                {
                    if (day % 2 == 0)
                    {
                        generatedDays.Add(CreateWorkDayDto(userRules, year, month, day, firstShift: true));
                    }
                    else
                    {
                        generatedDays.Add(CreateWorkDayDto(userRules, year, month, day, firstShift: false));
                    }
                }

                if (userRules.UnEvenDOM)
                {
                    if (day % 2 != 0)
                    {
                        generatedDays.Add(CreateWorkDayDto(userRules, year, month, day, firstShift: true));
                    }
                    else
                    {
                        generatedDays.Add(CreateWorkDayDto(userRules, year, month, day, firstShift: false));
                    }
                }
            }

            return generatedDays;
        }

        private static WorkDay CreateWorkDayDto(UserScheduleRules userRules, int year, int month, int day, bool firstShift)
        {
            DateTime startTime, endTime;
            if (firstShift)
            {
                startTime = new DateTime(year, month, day, 8, 0, 0);
                endTime = new DateTime(year, month, day, 14, 30, 0);
            }
            else
            {
                startTime = new DateTime(year, month, day, 14, 30, 0);
                endTime = new DateTime(year, month, day, 21, 0, 0);
            }

            return new WorkDay
            {
                Day = day,
                StartTime = startTime,
                EndTime = endTime,
            };
        }
    }
}