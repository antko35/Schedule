using Microsoft.AspNetCore.Identity;
using Moq;
using UserService.Application.DTOs;
using UserService.Domain.Models;
using Xunit;

namespace UserServiceTests.Application
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly UserService.Application.Services.UserService _userService;

        public UserServiceTests()
        {
            _userManagerMock = MockUserManager();
            _roleManagerMock = MockRoleManager();
            _userService = new UserService.Application.Services.UserService(
                _userManagerMock.Object,
                _roleManagerMock.Object
            );
        }

        [Fact]
        public async Task ChangeRole_UserNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new ChangeUserRole { Email = "nonexistent@example.com", Role = "Admin" };
            _userManagerMock
                .Setup(um => um.FindByEmailAsync(request.Email))
                .ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.ChangeRole(request));
            Assert.Equal($"email doesnt exist {request.Email}", exception.Message);
        }

        [Fact]
        public async Task ChangeRole_RoleNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = new User { Email = "test@example.com" };
            var request = new ChangeUserRole { Email = user.Email, Role = "NonexistentRole" };

            _userManagerMock
                .Setup(um => um.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);
            _roleManagerMock
                .Setup(rm => rm.FindByNameAsync(request.Role))
                .ReturnsAsync((IdentityRole)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.ChangeRole(request));
            Assert.Equal("Role " + request.Role + " doest exixt", exception.Message);
        }

        [Fact]
        public async Task ChangeRole_ValidInput_AddsUserToRole()
        {
            // Arrange
            var user = new User { Email = "test@example.com" };
            var role = new IdentityRole { Name = "Admin" };
            var request = new ChangeUserRole { Email = user.Email, Role = role.Name };

            _userManagerMock
                .Setup(um => um.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);
            _roleManagerMock
                .Setup(rm => rm.FindByNameAsync(role.Name))
                .ReturnsAsync(role);
            _userManagerMock
                .Setup(um => um.AddToRoleAsync(user, role.Name))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _userService.ChangeRole(request);

            // Assert
            _userManagerMock.Verify(um => um.AddToRoleAsync(user, role.Name), Times.Once);
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
