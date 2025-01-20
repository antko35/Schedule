namespace UserService.Application.Services
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using global::UserService.API.Grpc.Services;
    using global::UserService.Domain.Models;
    using Grpc.Core;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.Identity;

    public class GrpcService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        : UserGrpcService.UserGrpcServiceBase
    {
        public async override Task<AddClaimResponse> AddClaimToPerson(AddClaimRequest request, ServerCallContext context)
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return new AddClaimResponse
                {
                    Success = false,
                    Message = "No person with this email",
                };
            }

            var claim = new Claim(request.ClaimType, request.ClaimValue);

            var userClaims = await userManager.GetClaimsAsync(user);

            if (userClaims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
            {
                return new AddClaimResponse
                {
                    Success = false,
                    Message = "User already have this claim",
                };
            }

            var result = await userManager.AddClaimAsync(user, claim);

            if (result.Succeeded)
            {
                return new AddClaimResponse
                {
                    Success = true,
                    Message = "Claim added successfully",
                };
            }
            else
            {
                return new AddClaimResponse
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                };
            }
        }

        public async override Task<AddClaimResponse> DeletePersonClaim(AddClaimRequest request, ServerCallContext context)
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return new AddClaimResponse
                {
                    Success = false,
                    Message = "No user with this email",
                };
            }

            var claimToDelete = new Claim(request.ClaimType, request.ClaimValue);

            var userClaims = await userManager.GetClaimsAsync(user);

            if (!userClaims.Any(c => c.Type == claimToDelete.Type && c.Value == claimToDelete.Value))
            {
                return new AddClaimResponse
                {
                    Success = false,
                    Message = "User doesnt have this claim",
                };
            }

            var result = await userManager.RemoveClaimAsync(user, claimToDelete);

            if (result.Succeeded)
            {
                return new AddClaimResponse
                {
                    Success = true,
                    Message = "Claim successfully delited",
                };
            }
            else
            {
                return new AddClaimResponse
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                };
            }
        }

        public async override Task<GetClaimResponse> GetPersonClaims(GetClaimsRequest request, ServerCallContext context)
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return new GetClaimResponse
                {
                    Success = false,
                    Message = "No person with this email",
                };
            }

            var claims = await userManager.GetClaimsAsync(user);

            var response = new GetClaimResponse
            {
                Success = true,
                Message = "Claims retrieved successfully",
            };

            foreach (var claim in claims)
            {
                response.Claims.Add(new global::Claim
                {
                    Type = claim.Type,
                    Value = claim.Value,
                });
            }

            return response;
        }
    }
}
