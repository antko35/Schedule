﻿namespace UserManagementService.Application.UseCases.QueryHandlersTests.Department
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
    using FluentAssertions;

    public class GetDepartmentByIdTests
    {
        private readonly Mock<IDepartmentRepository> departmentRepositoryMock;
        private readonly GetDepartmentByIdQueryHandler handler;

        public GetDepartmentByIdTests()
        {
            departmentRepositoryMock = new Mock<IDepartmentRepository>();
            handler = new GetDepartmentByIdQueryHandler(departmentRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnDepartment_WhenDepartmentExists()
        {
            // Arrange
            var departmentId = ObjectId.GenerateNewId().ToString();
            var department = new Department
            {
                Id = departmentId,
                DepartmentName = "Department"
            };

            departmentRepositoryMock
                .Setup(repo => repo.GetByIdAsync(departmentId))
                .ReturnsAsync(department);

            var query = new GetDepartmentByIdQuery(departmentId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            departmentRepositoryMock.Verify(repo => repo.GetByIdAsync(departmentId), Times.Once);
            result.Should().NotBeNull();
            result.Id.Should().Be(department.Id);
            result.DepartmentName.Should().Be(department.DepartmentName);
        }

        [Fact]
        public async Task Handle_ShouldThrowKeyNotFoundException_WhenDepartmentDoesNotExist()
        {
            // Arrange
            var departmentId = ObjectId.GenerateNewId().ToString();

            departmentRepositoryMock
                .Setup(repo => repo.GetByIdAsync(departmentId))
                .ReturnsAsync((Department)null);

            var query = new GetDepartmentByIdQuery(departmentId);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(query, CancellationToken.None));

            // Assert
            exception.Message.Should().Be("Department not found");
            departmentRepositoryMock.Verify(repo => repo.GetByIdAsync(departmentId), Times.Once);
        }
    }
}
