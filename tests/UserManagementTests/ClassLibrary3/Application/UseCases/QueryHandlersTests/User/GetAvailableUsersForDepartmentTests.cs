namespace UserManagementService.Application.UseCases.QueryHandlersTests.User;

using AutoMapper;
using FluentAssertions;
using MongoDB.Bson;
using Moq;
using UserManagementService.Application.DTOs;
using UserManagementService.Application.UseCases.Queries.User;
using UserManagementService.Application.UseCases.QueryHandlers.User;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;
using Xunit;

public class GetAvailableUsersForDepartmentTests
{
    private readonly Mock<IDepartmentRepository> departmentRepositoryMock = new Mock<IDepartmentRepository>();
    private readonly Mock<IUserJobsRepository> userJobsRepositoryMock = new Mock<IUserJobsRepository>();
    private readonly Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
    private readonly Mock<IMapper> mapperMock = new Mock<IMapper>();

    [Fact]
    public async Task Handle_ShouldReturnAllUsers_WhenDepartmentHasNoMembers()
    {
        var departmentId = ObjectId.GenerateNewId().ToString();
        var availableUsers = CreateAvailableUsers();
        var response = CreateResponse(availableUsers);
        var cancellationToken = new CancellationTokenSource().Token;

        departmentRepositoryMock
            .Setup(repository => repository.GetByIdAsync(departmentId))
            .ReturnsAsync(new Department { Id = departmentId });
        userJobsRepositoryMock
            .Setup(repository => repository.GetUserJobsByDepartmentIdAsync(departmentId))
            .ReturnsAsync(Array.Empty<UserJob>());
        userRepositoryMock
            .Setup(repository => repository.GetAvailableUsersForDepartmentAsync(
                It.Is<IEnumerable<string>>(ids => !ids.Any()),
                cancellationToken))
            .ReturnsAsync(availableUsers);
        mapperMock
            .Setup(mapper => mapper.Map<IEnumerable<ShortUserInfoDto>>(availableUsers))
            .Returns(response);

        var result = await CreateHandler().Handle(
            new GetAvailableUsersForDepartmentQuery(departmentId),
            cancellationToken);

        result.Should().BeEquivalentTo(response, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Handle_ShouldExcludeExistingDepartmentMembers()
    {
        var departmentId = ObjectId.GenerateNewId().ToString();
        var firstExistingUserId = ObjectId.GenerateNewId().ToString();
        var secondExistingUserId = ObjectId.GenerateNewId().ToString();
        string[] membershipUserIds =
        {
            firstExistingUserId,
            firstExistingUserId,
            null!,
            string.Empty,
            secondExistingUserId,
        };
        var expectedExistingUserIds = new[] { firstExistingUserId, secondExistingUserId };
        var availableUsers = CreateAvailableUsers();
        var response = CreateResponse(availableUsers);

        departmentRepositoryMock
            .Setup(repository => repository.GetByIdAsync(departmentId))
            .ReturnsAsync(new Department { Id = departmentId });
        userJobsRepositoryMock
            .Setup(repository => repository.GetUserJobsByDepartmentIdAsync(departmentId))
            .ReturnsAsync(membershipUserIds.Select(userId => new UserJob { UserId = userId }));
        userRepositoryMock
            .Setup(repository => repository.GetAvailableUsersForDepartmentAsync(
                It.Is<IEnumerable<string>>(ids => ids.SequenceEqual(expectedExistingUserIds)),
                CancellationToken.None))
            .ReturnsAsync(availableUsers);
        mapperMock
            .Setup(mapper => mapper.Map<IEnumerable<ShortUserInfoDto>>(availableUsers))
            .Returns(response);

        var result = await CreateHandler().Handle(
            new GetAvailableUsersForDepartmentQuery(departmentId),
            CancellationToken.None);

        result.Should().BeEquivalentTo(response, options => options.WithStrictOrdering());
        userRepositoryMock.VerifyAll();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenEveryUserIsAlreadyInDepartment()
    {
        var departmentId = ObjectId.GenerateNewId().ToString();
        var existingUserId = ObjectId.GenerateNewId().ToString();
        var availableUsers = new List<ShortUserInfo>();
        var response = Array.Empty<ShortUserInfoDto>();

        departmentRepositoryMock
            .Setup(repository => repository.GetByIdAsync(departmentId))
            .ReturnsAsync(new Department { Id = departmentId });
        userJobsRepositoryMock
            .Setup(repository => repository.GetUserJobsByDepartmentIdAsync(departmentId))
            .ReturnsAsync(new[] { new UserJob { UserId = existingUserId } });
        userRepositoryMock
            .Setup(repository => repository.GetAvailableUsersForDepartmentAsync(
                It.Is<IEnumerable<string>>(ids => ids.SequenceEqual(new[] { existingUserId })),
                CancellationToken.None))
            .ReturnsAsync(availableUsers);
        mapperMock
            .Setup(mapper => mapper.Map<IEnumerable<ShortUserInfoDto>>(availableUsers))
            .Returns(response);

        var result = await CreateHandler().Handle(
            new GetAvailableUsersForDepartmentQuery(departmentId),
            CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenDepartmentDoesNotExist()
    {
        var departmentId = ObjectId.GenerateNewId().ToString();

        departmentRepositoryMock
            .Setup(repository => repository.GetByIdAsync(departmentId))
            .ReturnsAsync((Department)null!);

        var act = () => CreateHandler().Handle(
            new GetAvailableUsersForDepartmentQuery(departmentId),
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Department not found");
        userJobsRepositoryMock.Verify(
            repository => repository.GetUserJobsByDepartmentIdAsync(It.IsAny<string>()),
            Times.Never);
        userRepositoryMock.Verify(
            repository => repository.GetAvailableUsersForDepartmentAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static List<ShortUserInfo> CreateAvailableUsers()
    {
        return
        [
            new ShortUserInfo
            {
                Id = ObjectId.GenerateNewId().ToString(),
                FirstName = "Anna",
                LastName = "Ivanova",
                Patronymic = "Ivanovna",
            },
        ];
    }

    private static IEnumerable<ShortUserInfoDto> CreateResponse(IEnumerable<ShortUserInfo> users)
    {
        return users.Select(user => new ShortUserInfoDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Patronymic = user.Patronymic,
        });
    }

    private GetAvailableUsersForDepartmentQueryHandler CreateHandler()
    {
        return new GetAvailableUsersForDepartmentQueryHandler(
            departmentRepositoryMock.Object,
            userJobsRepositoryMock.Object,
            userRepositoryMock.Object,
            mapperMock.Object);
    }
}
