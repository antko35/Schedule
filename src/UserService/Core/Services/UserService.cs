namespace UserService.Application.Services
{
    using System.Security.Claims;
    using global::UserService.Application.DTOs;
    using global::UserService.Domain.Models;
    using Grpc.Core;
    using Microsoft.AspNetCore.Identity;

    public class UserService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        public async Task ChangeRole(ChangeUserRole request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new InvalidOperationException($"email doesnt exist {request.Email}");
            }

            var role = await roleManager.FindByNameAsync(request.Role);
            if (role == null)
            {
                throw new InvalidOperationException("Role " + request.Role + " doest exixt");
            }

            await userManager.AddToRoleAsync(user, role.Name!);
        }

        // public async Task<AddClaimResponse> AddClaimToPerson(AddClaimRequest request)
        // {
        // var user = await userManager.FindByEmailAsync(request.Email);
        //    if (user == null)
        //    {
        //        throw new InvalidOperationException($"No person with this email {request.Email}");
        //    }
        //    var claim = new Claim(request.organizationId, request.ClaimName);
        //// userManager.GetUsersForClaimAsync(claim).Wait();
        // var result = await userManager.AddClaimAsync(user, claim);
        //    if (result.Succeeded)
        //    {
        //        return new AddClaimResponse
        //        {
        //            Success = true,
        //            Message = "Claim added successfully"
        //        };
        //    }
        //    return new AddClaimResponse
        //    {
        //        Success = false,
        //        Message = string.Join(", ", result.Errors.Select(e => e.Description))
        //    };
// }
// public async Task<AddClaimResponse> DeletePersonClaim(AddClaimRequest request)
//        {
//            var user = await userManager.FindByEmailAsync(request.Email);
//            if (user == null)
//            {
//                throw new InvalidOperationException($"No person with this email {request.Email}");
//            }
//            var claimToDelete = new Claim(request.organizationId, request.ClaimName);
//            var result = await userManager.RemoveClaimAsync(user, claimToDelete);
//            if (result.Succeeded)
//            {
//                return new AddClaimResponse
//                {
//                    Success = true,
//                    Message = "Claim added successfully"
//                };
//            }
//            return new AddClaimResponse
//            {
//                Success = false,
//                Message = string.Join(", ", result.Errors.Select(e => e.Description))
//            };
//        }
   }
}
