namespace UserManagementService.Application.UseCases.CommandHandlersTests.User;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using UserManagementService.Application.UseCases.CommandHandlers.User;
using UserManagementService.Application.UseCases.Commands.User;
using UserManagementService.Domain.Abstractions.IRabbitMq;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;
using Xunit;

public class DeleteUserTests
{
    private readonly Mock<IUserRepository> userRepositoryMock;
    private readonly Mock<IUserJobsRepository> userJobsRepositoryMock;
    private readonly Mock<IUserEventPublisher> userEventPublisherMock;
    private readonly DeleteUserCommandHandler handler;
    private readonly DeleteUserCommand command;

    public DeleteUserTests()
    {
        userRepositoryMock = new Mock<IUserRepository>();
        userJobsRepositoryMock = new Mock<IUserJobsRepository>();
        userEventPublisherMock = new Mock<IUserEventPublisher>();

        this.handler = new DeleteUserCommandHandler(userRepositoryMock.Object, userJobsRepositoryMock.Object, userEventPublisherMock.Object);

        command = new DeleteUserCommand("userId");
    }

    [Fact]
    public async Task DeleteDepartment_Success()
    {
        // Arrange
        var user = new User { Id = command.UserId, FirstName = "John" };

        userRepositoryMock.Setup(repo => repo.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        userRepositoryMock.Setup(repo => repo.RemoveAsync(command.UserId))
            .Returns(Task.CompletedTask);

        userJobsRepositoryMock.Setup(repo => repo.DeleteByUserId(command.UserId))
            .Returns(Task.CompletedTask);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        userRepositoryMock.Verify(repo => repo.GetByIdAsync(command.UserId), Times.Once);
        userRepositoryMock.Verify(repo => repo.RemoveAsync(command.UserId), Times.Once);
        userJobsRepositoryMock.Verify(repo => repo.DeleteByUserId(command.UserId), Times.Once);
    }

    [Fact]
    public async Task Handle_UserDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        userRepositoryMock.Setup(repo => repo.GetByIdAsync(command.UserId))
            .ReturnsAsync((User)null);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found");

        userRepositoryMock.Verify(repo => repo.GetByIdAsync(command.UserId), Times.Once);
        userRepositoryMock.Verify(repo => repo.RemoveAsync(It.IsAny<string>()), Times.Never);
        userJobsRepositoryMock.Verify(repo => repo.DeleteByUserId(It.IsAny<string>()), Times.Never);
    }
}