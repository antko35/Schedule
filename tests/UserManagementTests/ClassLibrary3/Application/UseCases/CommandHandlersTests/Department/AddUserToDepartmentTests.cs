namespace UserManagementService.Application.UseCases.CommandHandlersTests.Department
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Moq;
    using UserManagementService.Application.UseCases.CommandHandlers.Department;
    using UserManagementService.Application.UseCases.Commands.Department;
    using UserManagementService.Domain.Abstractions.IRepository;
    using UserManagementService.Domain.Models;
    using Xunit;

    public class AddUserToDepartmentTests
    {
        private readonly Mock<IUserRepository> userRepositoryMock;
        private readonly Mock<IDepartmentRepository> departmentRepositoryMock;
        private readonly Mock<IUserJobsRepository> userJobsRepositoryMock;
        private readonly Mock<IClinicRepository> clinicRepositoryMock;
        private readonly AddUserToDepartmentCommandHandler handler;
        private readonly AddUserToDepartmentCommand command;

        public AddUserToDepartmentTests()
        {
            userRepositoryMock = new Mock<IUserRepository>();
            departmentRepositoryMock = new Mock<IDepartmentRepository>();
            userJobsRepositoryMock = new Mock<IUserJobsRepository>();
            clinicRepositoryMock = new Mock<IClinicRepository>();

            handler = new AddUserToDepartmentCommandHandler(
                userRepositoryMock.Object,
                departmentRepositoryMock.Object,
                userJobsRepositoryMock.Object,
                clinicRepositoryMock.Object);

            command = new AddUserToDepartmentCommand
            {
                UserId = "user",
                DepartmentId = "dept123",
                Role = "Doctor",
                Status = "Active",
                Email = "doctor@example.com",
                PhoneNumber = "123-456-7890"
            };
        }

        [Fact]
        public async Task AddUserToDepartment_Success_ReturnNewUser()
        {
            // Arrange
            var user = new User { Id = command.UserId };
            var department = new Department { Id = command.DepartmentId };

            userRepositoryMock.Setup(repo => repo.GetByIdAsync(command.UserId))
                .ReturnsAsync(user);

            departmentRepositoryMock.Setup(repo => repo.GetByIdAsync(command.DepartmentId))
                .ReturnsAsync(department);

            userJobsRepositoryMock.Setup(repo => repo.GetUserJobAsync(command.UserId, command.DepartmentId))
                .ReturnsAsync((UserJob)null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(command.UserId, result.UserId);
            Assert.Equal(command.DepartmentId, result.DepartmentId);
            Assert.Equal(command.Role, result.Role);
            Assert.Equal(command.Status, result.Status);

            userJobsRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<UserJob>()), Times.Once);
        }

        [Fact]
        public async Task AddUserToDepartment_UserNotFound_ThrowKeyNotFoundException()
        {
            // Arrange
            userRepositoryMock.Setup(repo => repo.GetByIdAsync(command.UserId))
                .ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Equal("User doesnt exist", exception.Message);
        }

        [Fact]
        public async Task AddUserToDepartment_DepartmentNotFound_ThrowKeyNotFoundException()
        {
            // Arrange
            var user = new User { Id = command.UserId };

            userRepositoryMock.Setup(repo => repo.GetByIdAsync(command.UserId))
                .ReturnsAsync(user);

            departmentRepositoryMock.Setup(repo => repo.GetByIdAsync(command.DepartmentId))
                .ReturnsAsync((Department)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Equal("Department doesnt exist", exception.Message);
        }

        [Fact]
        public async Task AddUserToDepartment_AlreadyInDepartment_ThrowKeyNotFoundException()
        {
            // Arrange
            var user = new User { Id = command.UserId };
            var department = new Department { Id = command.DepartmentId };
            var existingUserJob = new UserJob { UserId = command.UserId, DepartmentId = command.DepartmentId };

            userRepositoryMock.Setup(repo => repo.GetByIdAsync(command.UserId))
                .ReturnsAsync(user);

            departmentRepositoryMock.Setup(repo => repo.GetByIdAsync(command.DepartmentId))
                .ReturnsAsync(department);

            userJobsRepositoryMock.Setup(repo => repo.GetUserJobAsync(command.UserId, command.DepartmentId))
                .ReturnsAsync(existingUserJob);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Equal($"User {command.UserId} already in department {command.DepartmentId}", exception.Message);
        }
    }
}
