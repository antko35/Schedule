namespace UserManagementService.Application.UseCases.CommandHandlersTests.Department
{
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Metadata;
    using System.Text;
    using System.Threading.Tasks;
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

            command = new CreateDepartmentCommand
            {
                ClinicId = "123",
                DeartmentName = "newDep"
            };
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
            Assert.NotNull(result);
            Assert.Equal(command.ClinicId, result.ClinicId);
            Assert.Equal(command.DeartmentName, result.DepartmentName);

            departmentRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Department>()), Times.Once);
        }

        [Fact]
        public async Task CreateDepartment_Success_ClinicIdIsNullOrEmpty_ReturnNewDepartment()
        {
            // Arrange
            var customCommand = new CreateDepartmentCommand
            {
                ClinicId = null,
                DeartmentName = "Neurology"
            };

            departmentRepositoryMock.Setup(repo => repo.GetByClinicId(It.IsAny<string>()))
                .ReturnsAsync(new List<Department>());

            departmentRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Department>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(customCommand, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.ClinicId)); // Check that ClinicId is generated
            Assert.Equal(customCommand.DeartmentName, result.DepartmentName);

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
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(command, CancellationToken.None));

            // Assert
            Assert.Equal("This department already exist", exception.Message);
        }
    }
}
