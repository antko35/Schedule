namespace UserManagementService.Application.UseCases.QueryHandlersTests.User
{
    using FluentAssertions;
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
                Id = "userId",
                FirstName = "Alice",
                LastName = "Smith",
                Patronymic = "Johnson",
                Gender = "Female"
            };

            var userJobs = new List<UserJob>
            {
                new UserJob { UserId = query.UserId, DepartmentId = "1", Role = "Doctor" },
                new UserJob { UserId = query.UserId, DepartmentId = "2", Role = "Head" }
            };

            userRepositoryMock.Setup(repo => repo.GetByIdAsync(query.UserId))
                .ReturnsAsync(user);

            userJobsRepositoryMock.Setup(repo => repo.GetUserJobsByUserIdAsync(query.UserId))
                .ReturnsAsync(userJobs);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.FirstName.Should().Be(user.FirstName);
            result.LastName.Should().Be(user.LastName);
            result.Patronymic.Should().Be(user.Patronymic);
            result.Gender.Should().Be(user.Gender);
            result.Jobs.Should().Equal(userJobs);

            userRepositoryMock.Verify(repo => repo.GetByIdAsync(query.UserId), Times.Once);
            userJobsRepositoryMock.Verify(repo => repo.GetUserJobsByUserIdAsync(query.UserId), Times.Once);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            userRepositoryMock.Setup(repo => repo.GetByIdAsync(query.UserId))
                .ReturnsAsync((User)null);

            // Act
            var act = async () => await handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("User not found");

            userRepositoryMock.Verify(repo => repo.GetByIdAsync(query.UserId), Times.Once);
            userJobsRepositoryMock.Verify(repo => repo.GetUserJobsByUserIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NoJobsForUser_ReturnsUserInfoWithEmptyJobs()
        {
            // Arrange
            var user = new User
            {
                Id = query.UserId,
                FirstName = "Alice",
                LastName = "Smith",
                Patronymic = "Johnson",
                Gender = "Female"
            };

            userRepositoryMock.Setup(repo => repo.GetByIdAsync(query.UserId))
                .ReturnsAsync(user);

            userJobsRepositoryMock.Setup(repo => repo.GetUserJobsByUserIdAsync(query.UserId))
                .ReturnsAsync(new List<UserJob>());

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.FirstName.Should().Be(user.FirstName);
            result.LastName.Should().Be(user.LastName);
            result.Patronymic.Should().Be(user.Patronymic);
            result.Gender.Should().Be(user.Gender);
            result.Jobs.Should().BeEmpty();

            userRepositoryMock.Verify(repo => repo.GetByIdAsync(query.UserId), Times.Once);
            userJobsRepositoryMock.Verify(repo => repo.GetUserJobsByUserIdAsync(query.UserId), Times.Once);
        }
    }
}
