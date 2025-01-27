using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using UserService.Application.DTOs;
using UserService.Domain.Models;
using Xunit;

namespace UserServiceTests.Application
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<User>> userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> roleManagerMock;
        private readonly UserService.Application.Services.UserService userService;

        public UserServiceTests()
        {
            this.userManagerMock = MockUserManager();
            this.roleManagerMock = MockRoleManager();
            this.userService = new UserService.Application.Services.UserService(
                this.userManagerMock.Object,
                this.roleManagerMock.Object);
        }

        [Fact]
        public async Task ChangeRole_UserNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new ChangeUserRole { Email = "nonexistent@example.com", Role = "Admin" };
            userManagerMock
                .Setup(um => um.FindByEmailAsync(request.Email))
                .ReturnsAsync((User)null);

            // Act
            var act = () => userService.ChangeRole(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"email doesnt exist {request.Email}");
        }

        [Fact]
        public async Task ChangeRole_RoleNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = new User { Email = "test@example.com" };
            var request = new ChangeUserRole { Email = user.Email, Role = "NonexistentRole" };

            userManagerMock
                .Setup(um => um.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);
            roleManagerMock
                .Setup(rm => rm.FindByNameAsync(request.Role))
                .ReturnsAsync((IdentityRole)null);

            // Act
            var act = () => userService.ChangeRole(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Role " + request.Role + " doest exixt");
        }

        [Fact]
        public async Task ChangeRole_ValidInput_AddsUserToRole()
        {
            // Arrange
            var user = new User { Email = "test@example.com" };
            var role = new IdentityRole { Name = "Admin" };
            var request = new ChangeUserRole { Email = user.Email, Role = role.Name };

            userManagerMock
                .Setup(um => um.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);
            roleManagerMock
                .Setup(rm => rm.FindByNameAsync(role.Name))
                .ReturnsAsync(role);
            userManagerMock
                .Setup(um => um.AddToRoleAsync(user, role.Name))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await userService.ChangeRole(request);

            // Assert
            userManagerMock.Verify(um => um.AddToRoleAsync(user, role.Name), Times.Once);
        }

        private Mock<UserManager<User>> MockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private Mock<RoleManager<IdentityRole>> MockRoleManager()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(store.Object, null, null, null, null);
        }
    }
}
