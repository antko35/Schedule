using ScheduleService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;

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
            int daysInMonth = DateTime.DaysInMonth(year, month);

            var generatedDays = Generate(officialHolidays, transferDays, userRules, daysInMonth, year, month);

            generatedDays = CorrectGeneralTime(generatedDays, userRules);

            return generatedDays;
        }

        private static List<WorkDay> CorrectGeneralTime(List<WorkDay> workDays, UserScheduleRules userRules)

        {
            var workdaysCount = workDays.Count;
            var totalTime = workdaysCount * 6.6;

            if (totalTime != userRules.HoursPerMonth)
            {
                var timeDifference = userRules.HoursPerMonth - totalTime;
                var dayToCorrect = workDays[workdaysCount - 1];
                var newEndTime = dayToCorrect.EndTime.AddHours(timeDifference);

                dayToCorrect.EndTime = newEndTime;
            }

            return workDays;
        }

        private static List<WorkDay> Generate(IEnumerable<Calendar> officialHolidays, IEnumerable<Calendar> transferDays, UserScheduleRules userRules, int daysInMonth, int year, int month)
{
    var generatedDays = new List<WorkDay>();

    for (int day = 1; day <= daysInMonth; day++)
    {
        var currentDay = new DateTime(year, month, day);
        DayOfWeek dayOfWeek = currentDay.DayOfWeek;

        // Пропуск праздничных дней
        if (officialHolidays.Any(x => x.HolidayDayOfMonth == day))
        {
            continue;
        }

        // Проверка на перенесенные дни
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

        bool isFirstShift = false;

        if (userRules.OnlyFirstShift)
        {
            isFirstShift = true;
        }
        else if (userRules.OnlySecondShift)
        {
            isFirstShift = false;
        }
        else
        {
            if (userRules.EvenDOW)
            {
                if (dayOfWeek == DayOfWeek.Tuesday || dayOfWeek == DayOfWeek.Thursday)
                {
                    isFirstShift = true;
                }
            }

            if (userRules.UnEvenDOW)
            {
                if (dayOfWeek == DayOfWeek.Monday || dayOfWeek == DayOfWeek.Wednesday || dayOfWeek == DayOfWeek.Friday)
                {
                    isFirstShift = true;
                }
            }

            if (userRules.EvenDOM)
            {
                if (day % 2 == 0)
                {
                    isFirstShift = true;
                }
            }

            if (userRules.UnEvenDOM)
            {
                if (day % 2 != 0)
                {
                    isFirstShift = true;
                }
            }
        }

        if (isFirstShift)
        {
            generatedDays.Add(CreateWorkDayDto(userRules, year, month, day, firstShift: true));
        }
        else
        {
            generatedDays.Add(CreateWorkDayDto(userRules, year, month, day, firstShift: false));
        }
    }

    return generatedDays;
}

        private static WorkDay CreateWorkDayDto(UserScheduleRules userRules, int year, int month, int day, bool firstShift)
        {
            TimeOnly startWork = userRules.StartWorkDayTime;
            TimeOnly endWork = startWork.AddHours(6).AddMinutes(30);

            DateTime startTime, endTime;
            if (firstShift)
            {
                startTime = new DateTime(year, month, day, startWork.Hour, startWork.Minute, 0);
                endTime = new DateTime(year, month, day, endWork.Hour, endWork.Minute, 0);
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