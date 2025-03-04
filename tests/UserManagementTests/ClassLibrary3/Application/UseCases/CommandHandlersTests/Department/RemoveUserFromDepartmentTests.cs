using UserManagementService.Domain.Abstractions.IRabbitMq;

namespace UserManagementService.Application.UseCases.CommandHandlersTests.Department
{
    using FluentAssertions;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UserManagementService.Application.UseCases.CommandHandlers.Department;
    using UserManagementService.Application.UseCases.Commands.Department;
    using UserManagementService.Domain.Abstractions.IRepository;
    using UserManagementService.Domain.Models;
    using Xunit;

    public class RemoveUserFromDepartmentTests
    {
        private readonly Mock<IUserJobsRepository> userJobsRepositoryMock;
        private readonly Mock<IUserEventPublisher> userEventPublisher;
        private readonly RemoveUserFromDepartmentCommandHandler handler;
        private readonly RemoveUserFromDepartmentCommand command;

        public RemoveUserFromDepartmentTests()
        {
            userJobsRepositoryMock = new Mock<IUserJobsRepository>();
            userEventPublisher = new Mock<IUserEventPublisher>();
            handler = new RemoveUserFromDepartmentCommandHandler(userJobsRepositoryMock.Object, userEventPublisher.Object);

            command = new RemoveUserFromDepartmentCommand("user123", "department456");
        }

        [Fact]
        public async Task RemoveUserFromDepartment_Success_ReturnRemovedUserJob()
        {
            // Arrange
            var existingUserJob = new UserJob
            {
                Id = "job789",
                UserId = command.UserId,
                DepartmentId = command.DepartmentId
            };

            userJobsRepositoryMock.Setup(repo => repo.GetUserJobAsync(command.UserId, command.DepartmentId))
                .ReturnsAsync(existingUserJob);
            userJobsRepositoryMock.Setup(repo => repo.RemoveAsync(existingUserJob.Id))
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(command.UserId);
            result.DepartmentId.Should().Be(command.DepartmentId);

            userEventPublisher.Verify(pub => pub.PublishUserDeleted(command.UserId, command.DepartmentId));

            userJobsRepositoryMock.Verify(repo => repo.GetUserJobAsync(command.UserId, command.DepartmentId), Times.Once);
            userJobsRepositoryMock.Verify(repo => repo.RemoveAsync(existingUserJob.Id), Times.Once);
        }

        [Fact]
        public async Task RemoveUserFromDepartment_UserJobNotFound_ThrowInvalidOperationException()
        {
            // Arrange
            userJobsRepositoryMock.Setup(repo => repo.GetUserJobAsync(command.UserId, command.DepartmentId))
                .ReturnsAsync((UserJob)null);

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("User not found in this department");

            userJobsRepositoryMock.Verify(repo => repo.GetUserJobAsync(command.UserId, command.DepartmentId), Times.Once);
            userJobsRepositoryMock.Verify(repo => repo.RemoveAsync(It.IsAny<string>()), Times.Never);
        }
    }
}
