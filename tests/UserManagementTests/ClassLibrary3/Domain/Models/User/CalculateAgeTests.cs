namespace UserManagementService.Domain.Models.User
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Domain.Models;
    using FluentAssertions;
    using Xunit;

    public class CalculateAgeTests
    {
        [Theory]
        [InlineData("2000-01-01", 25)]
        [InlineData("2000-12-31", 24)]
        [InlineData("1990-06-15", 34)]
        public void CalculateAge_ShouldReturnCorrectAge(string dateOfBirthString, int expectedAge)
        {
            // Arrange
            var dateOfBirth = DateOnly.Parse(dateOfBirthString);
            var user = new User { DateOfBirth = dateOfBirth };

            // Act
            var calculatedAge = user.CalculateAge();

            // Assert
            calculatedAge.Should().Be(expectedAge);
        }

        [Fact]
        public void CalculateAge_ShouldHandleLeapYearCorrectly()
        {
            // Arrange
            var dateOfBirth = new DateOnly(2004, 2, 29); // Високосный год
            var user = new User { DateOfBirth = dateOfBirth };

            // Act
            var calculatedAge = user.CalculateAge();

            // Assert
            calculatedAge.Should().Be(21);
        }
    }
}
