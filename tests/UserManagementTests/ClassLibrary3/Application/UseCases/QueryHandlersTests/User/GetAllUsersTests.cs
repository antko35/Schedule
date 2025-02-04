namespace UserManagementService.Application.UseCases.QueryHandlersTests.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Moq;
    using UserManagementService.Application.UseCases.Queries.User;
    using UserManagementService.Application.UseCases.QueryHandlers.User;
    using UserManagementService.Domain.Abstractions.IRepository;
    using Xunit;
    using Domain.Models;
    using MongoDB.Bson;
    using FluentAssertions;

    public class GetAllUsersTests
    {
        private readonly Mock<IUserRepository> userRepositoryMock;
        private readonly GetAllUsersQueryHandler handler;

        public GetAllUsersTests()
        {
            userRepositoryMock = new Mock<IUserRepository>();
            handler = new GetAllUsersQueryHandler(userRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    FirstName = "John",
                    LastName = "Doe",
                    Patronymic = "MiddleName",
                    Gender = "Male",
                    DateOfBirth = new DateOnly(1990, 1, 1),
                },
                new User
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    FirstName = "Jane",
                    LastName = "Smith",
                    Patronymic = "MiddleName",
                    Gender = "Female",
                    DateOfBirth = new DateOnly(1995, 5, 15),
                }
            };

            userRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(users);

            var query = new GetAllUsersQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            userRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(users.Count());
            result.Should().Equal(users);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            // Arrange
            userRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<User>());

            var query = new GetAllUsersQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            userRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
            result.Should().BeEmpty();
        }
    }
}
