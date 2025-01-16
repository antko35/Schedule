namespace UserManagementService.Application.UseCases.CommandHandlersTests.Department
{
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
        private readonly RemoveUserFromDepartmentCommandHandler handler;
        private readonly RemoveUserFromDepartmentCommand command;

        public RemoveUserFromDepartmentTests()
        {
            userJobsRepositoryMock = new Mock<IUserJobsRepository>();
            handler = new RemoveUserFromDepartmentCommandHandler(userJobsRepositoryMock.Object);

            command = new RemoveUserFromDepartmentCommand("user123", "department456");
        }

        [Fact]
        public async Task RemoveUserFromDepartment_Success_ReturnRemovedUserJob()
        {
            // Arrange
            var existingUserJob = new UserJob
            {
                Id = "job789",
                UserId = command.userId,
                DepartmentId = command.departmentId
            };

            userJobsRepositoryMock.Setup(repo => repo.GetUserJobAsync(command.userId, command.departmentId))
                .ReturnsAsync(existingUserJob);
            userJobsRepositoryMock.Setup(repo => repo.RemoveAsync(existingUserJob.Id))
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(command.userId, result.UserId);
            Assert.Equal(command.departmentId, result.DepartmentId);

            userJobsRepositoryMock.Verify(repo => repo.GetUserJobAsync(command.userId, command.departmentId), Times.Once);
            userJobsRepositoryMock.Verify(repo => repo.RemoveAsync(existingUserJob.Id), Times.Once);
        }

        [Fact]
        public async Task RemoveUserFromDepartment_UserJobNotFound_ThrowInvalidOperationException()
        {
            // Arrange
            userJobsRepositoryMock.Setup(repo => repo.GetUserJobAsync(command.userId, command.departmentId))
                .ReturnsAsync((UserJob)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Equal("User doesnt found in this department", exception.Message);
            userJobsRepositoryMock.Verify(repo => repo.GetUserJobAsync(command.userId, command.departmentId), Times.Once);
            userJobsRepositoryMock.Verify(repo => repo.RemoveAsync(It.IsAny<string>()), Times.Never);
        }
    }
}
