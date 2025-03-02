using FluentAssertions;
using ScheduleService.Application.Services;
using ScheduleService.Domain.Models;
using Xunit;

namespace Application.Services;

public class ScheduleGeneratorTests
{
    public IEnumerable<int> unEvenDays = new List<int>()
    {
        1, 3, 5,
    };

    public IEnumerable<int> EvenDays = new List<int>()
    {
        2, 4,
    };

    [Fact]
    public void GenerateWorkDaysForUser_NoHolidays_NoTransferDays_ShouldGenerateCorrectWorkDays()
    {
        // Arrange
        var userRules = new UserScheduleRules
        {
            EvenDOW = true,
            HoursPerMonth = 132,
            StartWorkDayTime = new TimeOnly(8, 0),
        };

        int year = 2025;
        int month = 2;
        var officialHolidays = new List<Calendar>();
        var transferDays = new List<Calendar>();

        // Act
        var result = ScheduleGenerator.GenerateWorkDaysForUser(userRules, year, month, officialHolidays, transferDays);

        // Assert
        Assert.Equal(20, result.Count);
    }

    [Fact]
    public void GenerateWorkDaysForUser_WithHolidays_ShouldSkipHolidays()
    {
        // Arrange
        var userRules = new UserScheduleRules
        {
            EvenDOW = true,
            HoursPerMonth = 132,
            StartWorkDayTime = new TimeOnly(8, 0),
        };

        int year = 2025;
        int month = 5;
        var officialHolidays = new List<Calendar>
        {
            new Calendar { HolidayDayOfMonth = 1 },
            new Calendar { HolidayDayOfMonth = 9 },
        };
        var transferDays = new List<Calendar>();

        // Act
        var result = ScheduleGenerator.GenerateWorkDaysForUser(userRules, year, month, officialHolidays, transferDays);

        // Assert
        result.Count.Should().Be(20); // 22 дня - 2 праздника
    }

    [Fact]
    public void GenerateWorkDaysForUser_WithTransferDays_ShouldHandleTransferDays()
    {
        // Arrange
        var userRules = new UserScheduleRules
        {
            EvenDOW = true,
            HoursPerMonth = 132,
            StartWorkDayTime = new TimeOnly(8, 0),
        };

        int year = 2025;
        int month = 1;
        var officialHolidays = new List<Calendar>
        {
            new Calendar { HolidayDayOfMonth = 1 },
            new Calendar { HolidayDayOfMonth = 2 },
            new Calendar { HolidayDayOfMonth = 6, TransferDate = new DateOnly(2025, 1, 11), },
            new Calendar { HolidayDayOfMonth = 7 },
        };
        var transferDays = new List<Calendar>
        {
            new Calendar
            {
                 HolidayDayOfMonth = 6,
                 TransferDate = new DateOnly(2025, 1, 11),
            },
        };

        // Act
        var result = ScheduleGenerator.GenerateWorkDaysForUser(userRules, year, month, officialHolidays, transferDays);

        // Assert
        result.Count.Should().Be(20);
    }

    [Fact]
    public void GenerateWorkDaysForUser_CorrectGeneralTime()
    {
        // Arrange
        var userRules = new UserScheduleRules
        {
            EvenDOW = true,
            HoursPerMonth = 131,
            StartWorkDayTime = new TimeOnly(8, 0),
        };

        int year = 2025;
        int month = 1;
        var officialHolidays = new List<Calendar>
        {
            new Calendar { HolidayDayOfMonth = 1 },
            new Calendar { HolidayDayOfMonth = 2 },
            new Calendar { HolidayDayOfMonth = 6, TransferDate = new DateOnly(2025, 1, 11), },
            new Calendar { HolidayDayOfMonth = 7 },
        };
        var transferDays = new List<Calendar>
        {
            new Calendar
            {
                HolidayDayOfMonth = 6,
                TransferDate = new DateOnly(2025, 1, 11),
            },
        };

        // Act
        var result = ScheduleGenerator.GenerateWorkDaysForUser(userRules, year, month, officialHolidays, transferDays);

        // Assert
        var lastWorkDay = result.Last();
        var lastWorkDayLength = lastWorkDay.EndTime - lastWorkDay.StartTime;

        lastWorkDayLength.Hours.Should().Be(5);
        lastWorkDayLength.Minutes.Should().Be(30);
    }

    [Fact]
    public void GenerateWorkDaysForUser_FirstShiftRules_ShouldGenerateFirstShift()
    {
        // Arrange
        var userRules = new UserScheduleRules
        {
            OnlyFirstShift = true,
            HoursPerMonth = 132,
            StartWorkDayTime = new TimeOnly(9, 0),
        };

        int year = 2025;
        int month = 2;
        var officialHolidays = new List<Calendar>();
        var transferDays = new List<Calendar>();

        // Act
        var result = ScheduleGenerator.GenerateWorkDaysForUser(userRules, year, month, officialHolidays, transferDays);

        // Assert
        result.Should().AllSatisfy(x => x.StartTime.Hour.Should().Be(9));
    }

    [Fact]
    public void GenerateWorkDaysForUser_SecondShiftRules_ShouldGenerateSecondShift()
    {
        // Arrange
        var userRules = new UserScheduleRules
        {
            OnlySecondShift = true,
            HoursPerMonth = 132,
        };

        int year = 2025;
        int month = 2;
        var officialHolidays = new List<Calendar>();
        var transferDays = new List<Calendar>();

        // Act
        var result = ScheduleGenerator.GenerateWorkDaysForUser(userRules, year, month, officialHolidays, transferDays);

        // Assert
        result.Should().AllSatisfy(x => x.StartTime.Hour.Should().Be(14));
        result.Should().AllSatisfy(x => x.StartTime.Minute.Should().Be(30));
    }

    [Fact]
    public void GenerateWorkDaysForUser_EvenDOW()
    {
        // Arrange
        var userRules = new UserScheduleRules
        {
            EvenDOW = true,
            HoursPerMonth = 132,
            StartWorkDayTime = new TimeOnly(8, 0),
        };

        int year = 2025;
        int month = 2;
        var officialHolidays = new List<Calendar>();
        var transferDays = new List<Calendar>();

        // Act
        var result = ScheduleGenerator.GenerateWorkDaysForUser(userRules, year, month, officialHolidays, transferDays);

        // Assert
        foreach (var workDay in result)
        {
            int dayOfWeek = (int)workDay.StartTime.Date.DayOfWeek;

            if (EvenDays.Contains(dayOfWeek))
            {
                workDay.StartTime.Hour.Should().Be(8, $"On even days (Tue, Thu), the start time should be 8:00, but was {workDay.StartTime}");
            }
            else
            {
                workDay.StartTime.Hour.Should().Be(14, $"On odd days (Mon, Wed, Fri), the start time should be 14:00, but was {workDay.StartTime}");
                workDay.StartTime.Minute.Should().Be(30, $"On odd days (Mon, Wed, Fri), the start minute should be 30, but was {workDay.StartTime}");
            }
        }
    }

    [Fact]
    public void GenerateWorkDaysForUser_UnEvenDOW()
    {
        // Arrange
        var userRules = new UserScheduleRules
        {
            UnEvenDOW = true,
            HoursPerMonth = 132,
            StartWorkDayTime = new TimeOnly(8, 0),
        };

        int year = 2025;
        int month = 2;
        var officialHolidays = new List<Calendar>();
        var transferDays = new List<Calendar>();

        // Act
        var result = ScheduleGenerator.GenerateWorkDaysForUser(userRules, year, month, officialHolidays, transferDays);

        // Assert
        foreach (var workDay in result)
        {
            int dayOfWeek = (int)workDay.StartTime.Date.DayOfWeek;

            if (unEvenDays.Contains(dayOfWeek))
            {
                workDay.StartTime.Hour.Should().Be(8, $"On even days (Tue, Thu), the start time should be 8:00, but was {workDay.StartTime}");
            }
            else
            {
                workDay.StartTime.Hour.Should().Be(14, $"On odd days (Mon, Wed, Fri), the start time should be 14:00, but was {workDay.StartTime}");
                workDay.StartTime.Minute.Should().Be(30, $"On odd days (Mon, Wed, Fri), the start minute should be 30, but was {workDay.StartTime}");
            }
        }
    }

    [Fact]
    public void GenerateWorkDaysForUser_EvenDOM()
    {
        // Arrange
        var userRules = new UserScheduleRules
        {
            EvenDOM = true,
            HoursPerMonth = 132,
            StartWorkDayTime = new TimeOnly(8, 0),
        };

        int year = 2025;
        int month = 2;
        var officialHolidays = new List<Calendar>();
        var transferDays = new List<Calendar>();

        // Act
        var result = ScheduleGenerator.GenerateWorkDaysForUser(userRules, year, month, officialHolidays, transferDays);

        // Assert
        foreach (var workDay in result)
        {
            int dayOfMonth = workDay.StartTime.Date.Day;

            if (dayOfMonth % 2 == 0)
            {
                workDay.StartTime.Hour.Should().Be(8, $"On even days, the start time should be 8:00, but was {workDay.StartTime}");
            }
            else
            {
                workDay.StartTime.Hour.Should().Be(14, $"On odd days , the start time should be 14:00, but was {workDay.StartTime}");
                workDay.StartTime.Minute.Should().Be(30, $"On odd days , the start minute should be 30, but was {workDay.StartTime}");
            }
        }
    }

    [Fact]
    public void GenerateWorkDaysForUser_UnEvenDOM()
    {
        // Arrange
        var userRules = new UserScheduleRules
        {
            UnEvenDOM = true,
            HoursPerMonth = 132,
            StartWorkDayTime = new TimeOnly(8, 0),
        };

        int year = 2025;
        int month = 2;
        var officialHolidays = new List<Calendar>();
        var transferDays = new List<Calendar>();

        // Act
        var result = ScheduleGenerator.GenerateWorkDaysForUser(userRules, year, month, officialHolidays, transferDays);

        // Assert
        foreach (var workDay in result)
        {
            int dayOfMonth = workDay.StartTime.Date.Day;

            if (dayOfMonth % 2 != 0)
            {
                workDay.StartTime.Hour.Should().Be(8, $"On even days, the start time should be 8:00, but was {workDay.StartTime}");
            }
            else
            {
                workDay.StartTime.Hour.Should().Be(14, $"On odd days , the start time should be 14:00, but was {workDay.StartTime}");
                workDay.StartTime.Minute.Should().Be(30, $"On odd days , the start minute should be 30, but was {workDay.StartTime}");
            }
        }
    }
}
