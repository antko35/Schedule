namespace UserManagementService.Tests.Domain;

using UserManagementService.Domain.Models;
using Xunit;

public class CalculateAgeTests
{
    [Fact]
    public void BirthdayTodayIncrementsAge()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var user = new User { DateOfBirth = today.AddYears(-25) };
        Assert.Equal(25, user.CalculateAge());
        Assert.Equal(25, user.Age);
    }

    [Fact]
    public void BirthdayTomorrowHasNotIncrementedAge()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var user = new User { DateOfBirth = today.AddYears(-25).AddDays(1) };
        Assert.Equal(24, user.CalculateAge());
    }

    [Fact]
    public void LeapDayBirthRetainsDateAndUsesExistingAnniversaryRule()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var birth = new DateOnly(2004, 2, 29);
        var expected = today.Year - 2004;
        if (today < new DateOnly(today.Year, 2, DateTime.IsLeapYear(today.Year) ? 29 : 28)) expected--;
        var user = new User { DateOfBirth = birth };
        Assert.Equal(expected, user.CalculateAge());
        Assert.Equal(birth, user.DateOfBirth);
    }
}
