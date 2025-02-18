namespace UserManagementService.Application.UseCases.CommandHandlersTests.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Moq;
    using UserManagementService.Application.UseCases.CommandHandlers.User;
    using UserManagementService.Application.UseCases.Commands.User;
    using UserManagementService.Domain.Abstractions.IRepository;
    using Xunit;
    using Domain.Models;
    using MongoDB.Bson;
    using FluentAssertions;

    public class UpdateUserAgeTests
    {
        private readonly Mock<IUserRepository> userRepositoryMock;
        private readonly UpdateUserAgeCommandHandler handler;

        public UpdateUserAgeTests()
        {
            userRepositoryMock = new Mock<IUserRepository>();
            handler = new UpdateUserAgeCommandHandler(userRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldUpdateAgeForAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    FirstName = "John",
                    LastName = "Doe",
                    DateOfBirth = new DateOnly(1990, 1, 1),
                },
                new User
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    FirstName = "Jane",
                    LastName = "Smith",
                    DateOfBirth = new DateOnly(1985, 5, 15),
                }
            };

            userRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(users);

            userRepositoryMock
                .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            var command = new UpdateUserAgeCommand();

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            userRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
            userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Exactly(users.Count));

            users[0].Age.Should().Be(CalculateAge(users[0].DateOfBirth));
            users[1].Age.Should().Be(CalculateAge(users[1].DateOfBirth));
        }

        [Fact]
        public async Task Handle_ShouldNotFailWhenNoUsersExist()
        {
            // Arrange
            userRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<User>());

            var command = new UpdateUserAgeCommand();

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            userRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
            userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
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
