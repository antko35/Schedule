﻿namespace UserServiceTests.Application.GrpcTests
{
    using System.Security.Claims;
    using FluentAssertions;
    using Grpc.Core;
    using Microsoft.AspNetCore.Identity;
    using Moq;
    using UserService.Application.Services;
    using UserService.Domain.Models;
    using Xunit;

    public class DeletePersonClaimTests
    {
        private readonly Mock<UserManager<User>> userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> roleManagerMock;
        private readonly GrpcService grpcService;
        private ServerCallContext context;

        public DeletePersonClaimTests()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStoreMock.Object, null, null, null, null);

            grpcService = new GrpcService(userManagerMock.Object, roleManagerMock.Object);

            context = TestServerCallContext.Create(
                method: "AddClaimToPerson",
                host: "localhost");
        }

        [Fact]
        public async Task DeleteClaim_UserNotFound_ErrorResponse()
        {
            // Arrange
            var email = "user@gmail.com";
            AddClaimRequest request = new AddClaimRequest { Email = email, ClaimType = "value", ClaimValue = "value" };
            userManagerMock.Setup(um => um.FindByEmailAsync(email)).ReturnsAsync((User)null);

            // Act
            var result = await grpcService.DeletePersonClaim(request, context);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No user with this email");
        }

        [Fact]
        public async Task DeleteClaim_ClaimNotFoud_ErrorResponse()
        {
            // Arrange
            var email = "user@gmail.com";
            var claim = new System.Security.Claims.Claim("type", "value");
            User user = new User { Email = email };
            AddClaimRequest request = new AddClaimRequest { Email = email, ClaimType = claim.Type, ClaimValue = claim.Value };

            userManagerMock.Setup(um => um.FindByEmailAsync(email)).ReturnsAsync(user);
            userManagerMock.Setup(um => um.GetClaimsAsync(user)).ReturnsAsync(new Claim[0]);

            // Act
            var result = await grpcService.DeletePersonClaim(request, context);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("User doesnt have this claim");
        }

        [Fact]
        public async Task DeleteClaim_ReturnsSuccessResponse()
        {
            // Arrange
            var email = "user@gmail.com";
            var claim = new Claim("type", "value");
            User user = new User { Email = email };
            AddClaimRequest request = new AddClaimRequest { Email = email, ClaimType = claim.Type, ClaimValue = claim.Value };

            userManagerMock.Setup(um => um.FindByEmailAsync(email)).ReturnsAsync(user);
            userManagerMock.Setup(um => um.GetClaimsAsync(user)).ReturnsAsync([claim]);
            userManagerMock.Setup(um => um.RemoveClaimAsync(It.IsAny<User>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await grpcService.DeletePersonClaim(request, context);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Claim successfully delited");
        }

        [Fact]
        public async Task DeleteClaim_RemoveError_ReturnsErrorResponse()
        {
            // Arrange
            var email = "user@gmail.com";
            var claim = new Claim("type", "value");
            User user = new User { Email = email };
            AddClaimRequest request = new AddClaimRequest { Email = email, ClaimType = claim.Type, ClaimValue = claim.Value };

            userManagerMock.Setup(um => um.FindByEmailAsync(email)).ReturnsAsync(user);
            userManagerMock.Setup(um => um.GetClaimsAsync(user)).ReturnsAsync([claim]);
            userManagerMock.Setup(um => um.RemoveClaimAsync(It.IsAny<User>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Failed());

            // Act
            var result = await grpcService.DeletePersonClaim(request, context);

            // Assert
            result.Success.Should().BeFalse();
        }
    }
}
