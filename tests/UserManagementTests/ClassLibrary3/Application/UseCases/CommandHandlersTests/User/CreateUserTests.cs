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

    public class CreateUserTests
    {
        private readonly Mock<IUserRepository> userRepositoryMock;
        private readonly CreateUserCommandHandler handler;

        public CreateUserTests()
        {
            userRepositoryMock = new Mock<IUserRepository>();
            handler = new CreateUserCommandHandler(userRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateUserAndReturnCreatedUser()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Patronymic = "Middle",
                Gender = "Male",
                DateOfBirth = new DateOnly(1990, 1, 1)
            };

            User createdUser = null;

            userRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .Callback<User>(user => createdUser = user)
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
            Assert.NotNull(result);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
            Assert.Equal("Middle", result.Patronymic);
            Assert.Equal("Male", result.Gender);
            Assert.Equal(new DateOnly(1990, 1, 1), result.DateOfBirth);
            Assert.Equal(DateTime.Today.Year - command.DateOfBirth.Year - (DateTime.Today < new DateTime(DateTime.Today.Year, 1, 11) ? 1 : 0), result.Age);
            Assert.NotNull(createdUser);
            Assert.Equal(result, createdUser);
        }
    }
}
