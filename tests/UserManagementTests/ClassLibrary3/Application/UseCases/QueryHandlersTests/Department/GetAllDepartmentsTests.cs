namespace UserManagementService.Application.UseCases.QueryHandlersTests.Department
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Moq;
    using UserManagementService.Application.UseCases.Queries.Department;
    using UserManagementService.Application.UseCases.QueryHandlers.Department;
    using UserManagementService.Domain.Abstractions.IRepository;
    using Xunit;
    using Domain.Models;
    using MongoDB.Bson;

    public class GetAllDepartmentsTests
    {
        private readonly Mock<IDepartmentRepository> departmentRepositoryMock;
        private readonly GetAllDepartmentsQueryHandler handler;

        public GetAllDepartmentsTests()
        {
            departmentRepositoryMock = new Mock<IDepartmentRepository>();
            handler = new GetAllDepartmentsQueryHandler(departmentRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnAllDepartments_WhenDepartmentsExists()
        {
            // Arrange
            var departments = new List<Department>
            {
                new Department { Id = ObjectId.GenerateNewId().ToString(), DepartmentName = "Department 1" },
                new Department { Id = ObjectId.GenerateNewId().ToString(), DepartmentName = "Department 2" }
            };

            departmentRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(departments);

            var query = new GetAllDepartmentsQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            departmentRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
            Assert.NotNull(result);
            Assert.Equal(departments.Count, result.Count());
            Assert.Equal(departments, result);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoDepartmentsExist()
        {
            // Arrange
            departmentRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<Department>());

            var query = new GetAllDepartmentsQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            departmentRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
