namespace UserServiceTests.Application.GrpcTests
{
    using FluentAssertions;
    using Grpc.Core;
    using Microsoft.AspNetCore.Identity;
    using Moq;
    using UserService.Application.Services;
    using UserService.Domain.Models;
    using Xunit;

    public class GetPersonClaimsTests
    {
        private readonly Mock<UserManager<User>> userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> roleManagerMock;
        private readonly GrpcService grpcService;
        private ServerCallContext context;

        public GetPersonClaimsTests()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStoreMock.Object, null, null, null, null);

            grpcService = new GrpcService(userManagerMock.Object, roleManagerMock.Object);

            context = TestServerCallContext.Create(
                method: "GetPersonClaims");
        }

        [Fact]
        public async Task GetClaims_UserNotFound_ErrorResponse()
        {
            // Arrange
            var email = "user@gmail.com";
            var reqest = new GetClaimsRequest { Email = email};
            userManagerMock.Setup(um => um.FindByEmailAsync(reqest.Email)).ReturnsAsync((User)null);

            // Act
            var result = await grpcService.GetPersonClaims(reqest, context);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No person with this email");
        }

        [Fact]
        public async Task GetClaims_SuccessResponse()
        {
            // Arrange
            var email = "user@gmail.com";
            var reqest = new GetClaimsRequest { Email = email };
            User user = new User { Email = email };
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("Type1", "Value1"),
                new System.Security.Claims.Claim("Type2", "Value2"),
            };
            userManagerMock.Setup(um => um.FindByEmailAsync(reqest.Email)).ReturnsAsync(user);
            userManagerMock.Setup(um => um.GetClaimsAsync(user)).ReturnsAsync(claims);

            // Act
            var result = await grpcService.GetPersonClaims(reqest, context);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Claims retrieved successfully");
            result.Claims.Count.Should().Be(2);

            result.Claims.Should().Contain(c => c.Type == "Type1" && c.Value == "Value1");
            result.Claims.Should().Contain(c => c.Type == "Type2" && c.Value == "Value2");
        }
    }
}
