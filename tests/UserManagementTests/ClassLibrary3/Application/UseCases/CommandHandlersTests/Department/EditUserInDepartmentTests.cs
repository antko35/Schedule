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

    public class EditUserInDepartmentTests
    {
        private readonly Mock<IDepartmentRepository> departmentRepositoryMock;
        private readonly Mock<IUserJobsRepository> userJobsRepositoryMock;
        private readonly Mock<IUserRepository> userRepositoryMock;
        private readonly EditUserInDepartmentCommandHandler handler;
        private readonly EditUserInDepartmentCommand command;

        public EditUserInDepartmentTests()
        {
            departmentRepositoryMock = new Mock<IDepartmentRepository>();
            userJobsRepositoryMock = new Mock<IUserJobsRepository>();
            userRepositoryMock = new Mock<IUserRepository>();

            handler = new EditUserInDepartmentCommandHandler(userRepositoryMock.Object, departmentRepositoryMock.Object, userJobsRepositoryMock.Object);

            command = new EditUserInDepartmentCommand
            {
                DepartmentId = "DepId",
                UserId = "userId",
                Role = "newRole",
                Status = "newStatus",
                Email = "new@gmail.com",
                PhoneNumber = "1234567890",
            };
        }

        [Fact]
        public async Task EditUserInDepartment_Success_ReturnUpdatedUserJob()
        {
            // Arrange
            var existingUser = new User { Id = command.UserId };
            var existingDepartment = new Department { Id = command.DepartmentId };
            var existingUserJob = new UserJob
            {
                Id = "job789",
                UserId = command.UserId,
                DepartmentId = command.DepartmentId
            };

            userRepositoryMock.Setup(repo => repo.GetByIdAsync(command.UserId))
                .ReturnsAsync(existingUser);
            departmentRepositoryMock.Setup(repo => repo.GetByIdAsync(command.DepartmentId))
                .ReturnsAsync(existingDepartment);
            userJobsRepositoryMock.Setup(repo => repo.GetUserJobAsync(command.UserId, command.DepartmentId))
                .ReturnsAsync(existingUserJob);
            userJobsRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<UserJob>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(command.UserId, result.UserId);
            Assert.Equal(command.DepartmentId, result.DepartmentId);
            Assert.Equal(command.Role, result.Role);
            Assert.Equal(command.Status, result.Status);
            Assert.Equal(command.PhoneNumber, result.PhoneNumber);
            Assert.Equal(command.Email, result.Email);

            userJobsRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<UserJob>()), Times.Once);
        }

        [Fact]
        public async Task EditUserInDepartment_UserNotFound_ThrowKeyNotFoundException()
        {
            // Arrange
            userRepositoryMock.Setup(repo => repo.GetByIdAsync(command.UserId))
                .ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Equal("User not found", exception.Message);
        }

        [Fact]
        public async Task EditUserInDepartment_DepartmentNotFound_ThrowKeyNotFoundException()
        {
            // Arrange
            userRepositoryMock.Setup(repo => repo.GetByIdAsync(command.UserId))
                .ReturnsAsync(new User { Id = command.UserId });
            departmentRepositoryMock.Setup(repo => repo.GetByIdAsync(command.DepartmentId))
                .ReturnsAsync((Department)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Equal("Department not found", exception.Message);
        }

        [Fact]
        public async Task EditUserInDepartment_UserNotInDepartment_ThrowInvalidOperationException()
        {
            // Arrange
            userRepositoryMock.Setup(repo => repo.GetByIdAsync(command.UserId))
                .ReturnsAsync(new User { Id = command.UserId });
            departmentRepositoryMock.Setup(repo => repo.GetByIdAsync(command.DepartmentId))
                .ReturnsAsync(new Department { Id = command.DepartmentId });
            userJobsRepositoryMock.Setup(repo => repo.GetUserJobAsync(command.UserId, command.DepartmentId))
                .ReturnsAsync((UserJob)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Equal("User not found in this department", exception.Message);
        }
    }
}
