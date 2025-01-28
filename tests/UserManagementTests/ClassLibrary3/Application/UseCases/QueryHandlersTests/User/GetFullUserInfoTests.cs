namespace UserManagementService.Application.UseCases.QueryHandlersTests.User
{
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UserManagementService.Application.UseCases.Queries.Department;
    using UserManagementService.Application.UseCases.Queries.User;
    using UserManagementService.Application.UseCases.QueryHandlers.Department;
    using UserManagementService.Application.UseCases.QueryHandlers.User;
    using UserManagementService.Domain.Abstractions.IRepository;
    using UserManagementService.Domain.Models;
    using Xunit;

    public class GetFullUserInfoTests
    {
        private readonly Mock<IUserRepository> userRepositoryMock;
        private readonly Mock<IUserJobsRepository> userJobsRepositoryMock;
        private readonly GetFullUserInfoQueryHandler handler;
        private readonly GetFullUserInfoQuery query;

        public GetFullUserInfoTests()
        {
            userRepositoryMock = new Mock<IUserRepository>();
            userJobsRepositoryMock = new Mock<IUserJobsRepository>();

            handler = new GetFullUserInfoQueryHandler(
                userRepositoryMock.Object,
                userJobsRepositoryMock.Object);

            query = new GetFullUserInfoQuery("userId");
        }

        [Fact]
        public async Task Handle_UserExists_ReturnsFullUserInfo()
        {
            // Arrange
            var user = new User
            {
                Id = query.userId,
                FirstName = "Alice",
                LastName = "Smith",
                Patronymic = "Johnson",
                Age = 30,
                Gender = "Female"
            };

            var userJobs = new List<UserJob>
            {
                new UserJob { UserId = query.userId, DepartmentId = "1", Role = "Doctor" },
                new UserJob { UserId = query.userId, DepartmentId = "2", Role = "Head" }
            };

            userRepositoryMock.Setup(repo => repo.GetByIdAsync(query.userId))
                .ReturnsAsync(user);

            userJobsRepositoryMock.Setup(repo => repo.GetUserJobsByUserIdAsync(query.userId))
                .ReturnsAsync(userJobs);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.FirstName, result.FirstName);
            Assert.Equal(user.LastName, result.LastName);
            Assert.Equal(user.Patronymic, result.Patronymic);
            Assert.Equal(user.Age, result.Age);
            Assert.Equal(user.Gender, result.Gender);
            Assert.Equal(userJobs, result.Jobs);

            userRepositoryMock.Verify(repo => repo.GetByIdAsync(query.userId), Times.Once);
            userJobsRepositoryMock.Verify(repo => repo.GetUserJobsByUserIdAsync(query.userId), Times.Once);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            userRepositoryMock.Setup(repo => repo.GetByIdAsync(query.userId))
                .ReturnsAsync((User)null);

            // Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(query, CancellationToken.None));

            // Assert
            Assert.Equal("User not found", exception.Message);

            userRepositoryMock.Verify(repo => repo.GetByIdAsync(query.userId), Times.Once);
            userJobsRepositoryMock.Verify(repo => repo.GetUserJobsByUserIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NoJobsForUser_ReturnsUserInfoWithEmptyJobs()
        {
            // Arrange
            var user = new User
            {
                Id = query.userId,
                FirstName = "Alice",
                LastName = "Smith",
                Patronymic = "Johnson",
                Age = 30,
                Gender = "Female"
            };

            userRepositoryMock.Setup(repo => repo.GetByIdAsync(query.userId))
                .ReturnsAsync(user);

            userJobsRepositoryMock.Setup(repo => repo.GetUserJobsByUserIdAsync(query.userId))
                .ReturnsAsync(new List<UserJob>());

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.FirstName, result.FirstName);
            Assert.Equal(user.LastName, result.LastName);
            Assert.Equal(user.Patronymic, result.Patronymic);
            Assert.Equal(user.Age, result.Age);
            Assert.Equal(user.Gender, result.Gender);
            Assert.Empty(result.Jobs);

            userRepositoryMock.Verify(repo => repo.GetByIdAsync(query.userId), Times.Once);
            userJobsRepositoryMock.Verify(repo => repo.GetUserJobsByUserIdAsync(query.userId), Times.Once);
        }
    }
}
