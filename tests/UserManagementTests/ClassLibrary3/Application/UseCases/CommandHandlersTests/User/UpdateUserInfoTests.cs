namespace UserManagementService.Application.UseCases.CommandHandlersTests.User
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using MongoDB.Bson;
    using Moq;
    using UserManagementService.Application.UseCases.CommandHandlers.User;
    using UserManagementService.Application.UseCases.Commands.User;
    using UserManagementService.Domain.Abstractions.IRepository;
    using UserManagementService.Domain.Models;
    using Xunit;

    public class UpdateUserInfoTests
    {
        private readonly Mock<IUserRepository> userRepositoryMock;
        private readonly UpdateUserInfoCommandHandler handler;

        public UpdateUserInfoTests()
        {
            userRepositoryMock = new Mock<IUserRepository>();
            handler = new UpdateUserInfoCommandHandler(userRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldUpdateUserInfo()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var existingUser = CreateTestUser(userId, "John", "Doe", "OldMiddle", "Male", new DateOnly(1990, 1, 1));

            var command = new UpdateUserInfoCommand
            {
                UserId = userId,
                FirstName = "UpdatedFirstName",
                LastName = "UpdatedLastName",
                Patronymic = "UpdatedMiddle",
                Gender = "Female",
                DateOfBirth = new DateOnly(1995, 5, 15)
            };

            userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);

            userRepositoryMock
                .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
            userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);

            result.Should().NotBeNull();
            result.Id.Should().Be(userId);
            result.FirstName.Should().Be("UpdatedFirstName");
            result.LastName.Should().Be("UpdatedLastName");
            result.Patronymic.Should().Be("UpdatedMiddle");
            result.Gender.Should().Be("Female");
            result.DateOfBirth.Should().Be(new DateOnly(1995, 5, 15));
            result.Age.Should().Be(CalculateAge(result.DateOfBirth));
        }

        [Fact]
        public async Task Handle_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var command = new UpdateUserInfoCommand
            {
                UserId = userId,
                FirstName = "NewFirstName",
                LastName = "NewLastName",
                Patronymic = "NewMiddle",
                Gender = "Non-Binary",
                DateOfBirth = new DateOnly(2000, 6, 30)
            };

            userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));

            // Assert
            userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
            userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);

            exception.Message.Should().Be("User not found");
        }

        private User CreateTestUser(string id, string firstName, string lastName, string patronymic, string gender, DateOnly dateOfBirth)
        {
            return new User
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                Patronymic = patronymic,
                Gender = gender,
                DateOfBirth = dateOfBirth,
                Age = CalculateAge(dateOfBirth)
            };
        }

        private int CalculateAge(DateOnly dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;

            if (today < new DateTime(today.Year, dateOfBirth.Month, dateOfBirth.Day))
            {
                age--;
            }

            return age;
        }
    }
}
