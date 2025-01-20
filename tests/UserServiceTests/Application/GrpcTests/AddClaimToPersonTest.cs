namespace UserServiceTests.Application.GrpcTests
{
    using System.Security.Claims;
    using Grpc.Core;
    using Microsoft.AspNetCore.Identity;
    using Moq;
    using UserService.Application.Services;
    using UserService.Domain.Models;
    using Xunit;

    public class AddClaimToPersonTest
    {
        private readonly Mock<UserManager<User>> userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> roleManagerMock;
        private readonly GrpcService grpcService;
        private ServerCallContext context;

        public AddClaimToPersonTest()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStoreMock.Object, null, null, null, null);

            grpcService = new GrpcService(userManagerMock.Object, roleManagerMock.Object);

            context = TestServerCallContext.Create(
                method: "AddClaimToPerson",
                host: "localhost"
            );
        }

        [Fact]
        public async Task AddClaim_UserNotFound_ErrorResponse()
        {
            var email = "user@gmail.com";
            var reqest = new AddClaimRequest { Email=email, ClaimType="value", ClaimValue = "value" };
            userManagerMock.Setup(um => um.FindByEmailAsync(reqest.Email)).ReturnsAsync((User)null);

            var result = await grpcService.AddClaimToPerson(reqest, context);

            Assert.False(result.Success);
            Assert.Equal("No person with this email", result.Message);
        }

        [Fact]
        public async Task AddClaim_ClaimAlreadyAdded_ErrorResponse()
        {
            var email = "user@gmail.com";
            var user = new User { Email = email };
            var claim = new Claim("type", "value");
            var reqest = new AddClaimRequest { Email = email, ClaimType = "type", ClaimValue = "value" };

            userManagerMock.Setup(um => um.FindByEmailAsync(email)).ReturnsAsync(user);
            userManagerMock.Setup(um => um.GetClaimsAsync(user)).ReturnsAsync([claim]);

            var result = await grpcService.AddClaimToPerson(reqest, this.context);

            Assert.False(result.Success);
            Assert.Equal("User already have this claim", result.Message);
        }

        [Fact]
        public async Task AddClaim_ReturnsSuccessResponse()
        {
            var email = "user@gmail.com";
            var user = new User { Email = email };
            var claim = new Claim("type", "value");
            var reqest = new AddClaimRequest { Email = email, ClaimType = "type", ClaimValue = "value" };

            userManagerMock.Setup(um => um.FindByEmailAsync(email)).ReturnsAsync(user);
            userManagerMock.Setup(um => um.GetClaimsAsync(user)).ReturnsAsync(new Claim[0]);
            userManagerMock.Setup(um => um.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);

            var result = await grpcService.AddClaimToPerson(reqest, this.context);

            Assert.True(result.Success);
            Assert.Equal("Claim added successfully", result.Message);
        }

        [Fact]
        public async Task AddClaim_AddingError_ReturnsErrorResponse()
        {
            var email = "user@gmail.com";
            var user = new User { Email = email };
            var claim = new Claim("type", "value");
            var reqest = new AddClaimRequest { Email = email, ClaimType = "type", ClaimValue = "value" };

            userManagerMock.Setup(um => um.FindByEmailAsync(email)).ReturnsAsync(user);
            userManagerMock.Setup(um => um.GetClaimsAsync(user)).ReturnsAsync(new Claim[0]);
            userManagerMock.Setup(um => um.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Failed());

            var result = await grpcService.AddClaimToPerson(reqest, this.context);

            Assert.False(result.Success);
        }
    }
}
