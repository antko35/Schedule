namespace UserManagementService.Application.UseCases.QueryHandlersTests.Department
{
    using FluentAssertions;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UserManagementService.Application.UseCases.Queries.Department;
    using UserManagementService.Application.UseCases.QueryHandlers.Department;
    using UserManagementService.Domain.Abstractions.IRepository;
    using UserManagementService.Domain.Models;
    using Xunit;

    public class GetUsersByDepartmentTests
    {
        private readonly Mock<IDepartmentRepository> departmentRepositoryMock;
        private readonly Mock<IUserRepository> userRepositoryMock;
        private readonly Mock<IUserJobsRepository> userJobsRepositoryMock;
        private readonly GetUsersByDepartmentQueryHandler handler;
        private readonly GetUsersByDepartmentQuery query;

        public GetUsersByDepartmentTests()
        {
            departmentRepositoryMock = new Mock<IDepartmentRepository>();
            userRepositoryMock = new Mock<IUserRepository>();
            userJobsRepositoryMock = new Mock<IUserJobsRepository>();

            handler = new GetUsersByDepartmentQueryHandler(
                departmentRepositoryMock.Object,
                userRepositoryMock.Object,
                userJobsRepositoryMock.Object);

            query = new GetUsersByDepartmentQuery("depId");
        }

        [Fact]
        public async Task GetUsersByDepartment_DepartmentExists_ReturnsUsers()
        {
            // Arrange
            var department = new Department { Id = query.departmentId, DepartmentName = "depName" };
            var userJobs = new List<UserJob>
            {
                new UserJob { UserId = "1", DepartmentId = query.departmentId, Role = "Doctor" },
                new UserJob { UserId = "2", DepartmentId = query.departmentId, Role = "Head" }
            };
            var users = new List<User>
            {
                new User { Id = "1", FirstName = "Alice" },
                new User { Id = "2", FirstName = "Bob" }
            };

            departmentRepositoryMock.Setup(repo => repo.GetByIdAsync(query.departmentId))
                .ReturnsAsync(department);

            userJobsRepositoryMock.Setup(repo => repo.GetUserJobsByDepartmentIdAsync(query.departmentId))
                .ReturnsAsync(userJobs);

            userRepositoryMock.Setup(repo => repo.GetByIdAsync("1"))
                .ReturnsAsync(users[0]);
            userRepositoryMock.Setup(repo => repo.GetByIdAsync("2"))
                .ReturnsAsync(users[1]);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
            result.Should().Contain(u => u.FirstName == "Alice" && u.Role == "Doctor");
            result.Should().Contain(u => u.FirstName == "Bob" && u.Role == "Head");

            departmentRepositoryMock.Verify(repo => repo.GetByIdAsync(query.departmentId), Times.Once);
            userJobsRepositoryMock.Verify(repo => repo.GetUserJobsByDepartmentIdAsync(query.departmentId), Times.Once);
            userRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetUsersByDepartment_DepartmentDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            departmentRepositoryMock.Setup(repo => repo.GetByIdAsync(query.departmentId))
                .ReturnsAsync((Department)null);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(query, CancellationToken.None));

            // Assert
            exception.Message.Should().Be("Department doesnt exist");

            departmentRepositoryMock.Verify(repo => repo.GetByIdAsync(query.departmentId), Times.Once);
            userJobsRepositoryMock.Verify(repo => repo.GetUserJobsByDepartmentIdAsync(It.IsAny<string>()), Times.Never);
            userRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetUsersByDepartment_NoUsersInDepartment_ReturnsEmptyList()
        {
            // Arrange
            var department = new Department { Id = query.departmentId, DepartmentName = "dep" };

            departmentRepositoryMock.Setup(repo => repo.GetByIdAsync(query.departmentId))
                .ReturnsAsync(department);

            userJobsRepositoryMock.Setup(repo => repo.GetUserJobsByDepartmentIdAsync(query.departmentId))
                .ReturnsAsync(new List<UserJob>());

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEmpty();

            departmentRepositoryMock.Verify(repo => repo.GetByIdAsync(query.departmentId), Times.Once);
            userJobsRepositoryMock.Verify(repo => repo.GetUserJobsByDepartmentIdAsync(query.departmentId), Times.Once);
            userRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<string>()), Times.Never);
        }
    }
}
