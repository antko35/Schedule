namespace UserServiceTests.Application.GrpcTests
{
    using System.Security.Claims;
    using Grpc.Core;
    using Microsoft.AspNetCore.Identity;
    using Moq;
    using UserService.Application.Services;
    using UserService.Domain.Models;
    using Xunit;

    public class DeletePersonClaimTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly GrpcService _grpcService;
        public ServerCallContext context;

        public DeletePersonClaimTests()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStoreMock.Object, null, null, null, null);

            _grpcService = new GrpcService(_userManagerMock.Object, _roleManagerMock.Object);

            context = TestServerCallContext.Create(
                method: "AddClaimToPerson",
                host: "localhost"
            );
        }

        [Fact]
        public async Task DeleteClaim_UserNotFound_ErrorResponse()
        {
            var email = "user@gmail.com";
            AddClaimRequest request = new AddClaimRequest { Email = email, ClaimType = "value", ClaimValue = "value" };
            _userManagerMock.Setup(um => um.FindByEmailAsync(email)).ReturnsAsync((User)null);

            var result = await _grpcService.DeletePersonClaim(request, context);

            Assert.False(result.Success);
            Assert.Equal("No user with this email", result.Message);
        }

        [Fact]
        public async Task DeleteClaim_ClaimNotFoud_ErrorResponse()
        {
            var email = "user@gmail.com";
            var claim = new System.Security.Claims.Claim("type", "value");
            User user = new User { Email = email };
            AddClaimRequest request = new AddClaimRequest { Email = email, ClaimType = claim.Type, ClaimValue = claim.Value };

            _userManagerMock.Setup(um => um.FindByEmailAsync(email)).ReturnsAsync(user);
            _userManagerMock.Setup(um => um.GetClaimsAsync(user)).ReturnsAsync(new Claim[0]);

            var result = await _grpcService.DeletePersonClaim(request, context);

            Assert.False(result.Success);
            Assert.Equal("User doesnt have this claim", result.Message);
        }

        [Fact]
        public async Task DeleteClaim_ReturnsSuccessResponse()
        {
            var email = "user@gmail.com";
            var claim = new Claim("type", "value");
            User user = new User { Email = email };
            AddClaimRequest request = new AddClaimRequest { Email = email, ClaimType = claim.Type, ClaimValue = claim.Value };

            _userManagerMock.Setup(um => um.FindByEmailAsync(email)).ReturnsAsync(user);
            _userManagerMock.Setup(um => um.GetClaimsAsync(user)).ReturnsAsync([claim]);
            _userManagerMock.Setup(um => um.RemoveClaimAsync(It.IsAny<User>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);

            var result = await _grpcService.DeletePersonClaim(request, context);

            Assert.True(result.Success);
            Assert.Equal("Claim successfully delited", result.Message);
        }

        [Fact]
        public async Task DeleteClaim_RemoveError_ReturnsErrorResponse()
        {
            var email = "user@gmail.com";
            var claim = new Claim("type", "value");
            User user = new User { Email = email };
            AddClaimRequest request = new AddClaimRequest { Email = email, ClaimType = claim.Type, ClaimValue = claim.Value };

            _userManagerMock.Setup(um => um.FindByEmailAsync(email)).ReturnsAsync(user);
            _userManagerMock.Setup(um => um.GetClaimsAsync(user)).ReturnsAsync([claim]);
            _userManagerMock.Setup(um => um.RemoveClaimAsync(It.IsAny<User>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Failed());

            var result = await _grpcService.DeletePersonClaim(request, context);

            Assert.False(result.Success);
        }
    }
}
