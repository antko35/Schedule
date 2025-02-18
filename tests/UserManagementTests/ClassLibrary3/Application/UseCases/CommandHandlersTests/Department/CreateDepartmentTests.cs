namespace UserManagementService.Application.UseCases.CommandHandlersTests.Department
{
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using UserManagementService.Application.UseCases.CommandHandlers.Department;
    using UserManagementService.Application.UseCases.Commands.Department;
    using UserManagementService.Domain.Abstractions.IRepository;
    using UserManagementService.Domain.Models;
    using Xunit;

    public class CreateDepartmentTests
    {
        private readonly Mock<IDepartmentRepository> departmentRepositoryMock;
        private readonly CreateDepartmentCommandHandler handler;
        private readonly CreateDepartmentCommand command;

        public CreateDepartmentTests()
        {
            departmentRepositoryMock = new Mock<IDepartmentRepository>();
            handler = new CreateDepartmentCommandHandler(departmentRepositoryMock.Object);

            command = new CreateDepartmentCommand("newDep", "123");
        }

        [Fact]
        public async Task CreateDepartment_Success_ReturnNewDepartment()
        {
            // Arrange
            departmentRepositoryMock.Setup(repo => repo.GetByClinicId(command.ClinicId))
                .ReturnsAsync(new List<Department>()); // No existing departments

            departmentRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Department>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ClinicId.Should().Be(command.ClinicId);
            result.DepartmentName.Should().Be(command.DeartmentName);

            departmentRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Department>()), Times.Once);
        }

        [Fact]
        public async Task CreateDepartment_Success_ClinicIdIsNullOrEmpty_ReturnNewDepartment()
        {
            // Arrange
            var customCommand = new CreateDepartmentCommand("Neurology", null);

            departmentRepositoryMock.Setup(repo => repo.GetByClinicId(It.IsAny<string>()))
                .ReturnsAsync(new List<Department>());

            departmentRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Department>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(customCommand, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ClinicId.Should().NotBeNullOrEmpty(); // Check that ClinicId is generated
            result.DepartmentName.Should().Be(customCommand.DeartmentName);

            departmentRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Department>()), Times.Once);
        }

        [Fact]
        public async Task CreateDepartment_DepartmentAlreadyExists_ThrowInvalidOperationException()
        {
            // Arrange
            var existingDepartments = new List<Department>
            {
                new Department { ClinicId = command.ClinicId, DepartmentName = command.DeartmentName }
            };

            departmentRepositoryMock.Setup(repo => repo.GetByClinicId(command.ClinicId))
                .ReturnsAsync(existingDepartments);

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("This department already exist");
        }
    }
}
