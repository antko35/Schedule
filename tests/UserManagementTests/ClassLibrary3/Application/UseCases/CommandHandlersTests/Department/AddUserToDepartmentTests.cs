using UserManagementService.Domain.Abstractions.IRabbitMq;

namespace UserManagementService.Application.UseCases.CommandHandlersTests.Department
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
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
        private readonly Mock<IUserEventPublisher> userEventPublisher;
        private readonly AddUserToDepartmentCommandHandler handler;
        private readonly AddUserToDepartmentCommand command;

        public AddUserToDepartmentTests()
        {
            userRepositoryMock = new Mock<IUserRepository>();
            departmentRepositoryMock = new Mock<IDepartmentRepository>();
            userJobsRepositoryMock = new Mock<IUserJobsRepository>();
            clinicRepositoryMock = new Mock<IClinicRepository>();
            userEventPublisher = new Mock<IUserEventPublisher>();

            handler = new AddUserToDepartmentCommandHandler(
                userRepositoryMock.Object,
                departmentRepositoryMock.Object,
                userJobsRepositoryMock.Object,
                userEventPublisher.Object);

            command = new AddUserToDepartmentCommand
                ("user", "dept123", "Doctor", "Active", "doctor@example.com", "123-456-7890");
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
            result.Should().NotBeNull();
            result.UserId.Should().Be(command.UserId);
            result.DepartmentId.Should().Be(command.DepartmentId);
            result.Role.Should().Be(command.Role);
            result.Status.Should().Be(command.Status);

            userEventPublisher.Verify(pub => pub.PublishUserCreated(command.UserId, command.DepartmentId));
            userJobsRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<UserJob>()), Times.Once);
        }

        [Fact]
        public async Task AddUserToDepartment_UserNotFound_ThrowKeyNotFoundException()
        {
            // Arrange
            userRepositoryMock.Setup(repo => repo.GetByIdAsync(command.UserId))
                .ReturnsAsync((User)null);

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("User not found");
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

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Department not found");
        }

        [Fact]
        public async Task AddUserToDepartment_AlreadyInDepartment_ThrowInvalidOperationException()
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

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"User {command.UserId} already in department {command.DepartmentId}");
        }
    }
}
