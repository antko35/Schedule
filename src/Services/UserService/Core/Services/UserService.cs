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

            var result = await userManager.AddToRoleAsync(user, role.Name!);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to add user '{request.Email}' to role '{role.Name}'. " +
                    $"Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}
