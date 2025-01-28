namespace UserManagementService.Application.UseCases.QueryHandlersTests.Department
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using MongoDB.Bson;
    using Moq;
    using UserManagementService.Application.UseCases.Queries.Department;
    using UserManagementService.Application.UseCases.QueryHandlers.Department;
    using UserManagementService.Domain.Abstractions.IRepository;
    using UserManagementService.Domain.Models;
    using Xunit;

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
        public async Task Handle_ShouldReturnAllDepartments_WhenDepartmentsExist()
        {
            // Arrange
            var departments = CreateTestDepartments(2);
            departmentRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(departments);

            var query = new GetAllDepartmentsQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            departmentRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);

            result.Should().NotBeNull()
                  .And.HaveCount(departments.Count)
                  .And.BeEquivalentTo(departments);
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

            result.Should().NotBeNull().And.BeEmpty();
        }

        private List<Department> CreateTestDepartments(int count)
        {
            var departments = new List<Department>();

            for (int i = 1; i <= count; i++)
            {
                departments.Add(new Department
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    DepartmentName = $"Department {i}"
                });
            }

            return departments;
        }
    }
}
