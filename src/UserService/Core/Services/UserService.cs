using Microsoft.AspNetCore.Identity;
using UserService.Application.DTOs;
using UserService.Domain.Models;

namespace UserService.Application.Services
{
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
    }
}
