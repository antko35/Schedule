using UserManagementService.Domain.Abstractions.IRabbitMq;

namespace UserManagementService.Application.UseCases.CommandHandlersTests.User
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using UserManagementService.Application.UseCases.CommandHandlers.User;
    using UserManagementService.Application.UseCases.Commands.User;
    using UserManagementService.Domain.Abstractions.IRepository;
    using UserManagementService.Domain.Models;
    using Xunit;

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
            var command = new CreateUserCommand("John", "Doe", "Middle", "Male", new DateOnly(1990, 1, 1));

            User createdUser = null;

            userRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .Callback<User>(user => createdUser = user)
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);

            createdUser.Should().NotBeNull();
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(createdUser);

            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.Patronymic.Should().Be("Middle");
            result.Gender.Should().Be("Male");
            result.DateOfBirth.Should().Be(new DateOnly(1990, 1, 1));

            var expectedAge = DateTime.Today.Year - command.DateOfBirth.Year -
                              (DateTime.Today < new DateTime(DateTime.Today.Year, command.DateOfBirth.Month, command.DateOfBirth.Day) ? 1 : 0);
            result.Age.Should().Be(expectedAge);
        }
    }
}
