namespace UserManagementService.Application.UseCases.CommandHandlersTests.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MongoDB.Bson;
    using Moq;
    using UserManagementService.Application.UseCases.CommandHandlers.User;
    using UserManagementService.Application.UseCases.Commands.User;
    using UserManagementService.Domain.Abstractions.IRepository;
    using Xunit;
    using Domain.Models;

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
            var existingUser = new User
            {
                Id = userId,
                FirstName = "John",
                LastName = "Doe",
                Patronymic = "OldMiddle",
                Gender = "Male",
                DateOfBirth = new DateOnly(1990, 1, 1),
                Age = 0
            };

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

            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("UpdatedFirstName", result.FirstName);
            Assert.Equal("UpdatedLastName", result.LastName);
            Assert.Equal("UpdatedMiddle", result.Patronymic);
            Assert.Equal("Female", result.Gender);
            Assert.Equal(new DateOnly(1995, 5, 15), result.DateOfBirth);
            Assert.Equal(DateTime.Today.Year - 1995 - (DateTime.Today < new DateTime(DateTime.Today.Year, 5, 15) ? 1 : 0), result.Age);
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

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));

            Assert.Equal("User not found", exception.Message);
            userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
            userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        }
    }
}
