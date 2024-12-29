namespace UserService.Infrastructure.Seeders
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using UserService.Domain.Constants;
    using UserService.Domain.Models;

    public class UserSeeder(UserDbContext dbContext, RoleManager<IdentityRole> roleManager) : IUserSeeder
    {
        public async Task Seed()
        {
            if (await dbContext.Database.CanConnectAsync())
            {
                var roles = GetRoles();
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role.Name))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role.Name));
                    }
                }
            }
        }

        private IEnumerable<IdentityRole> GetRoles()
        {
            List<IdentityRole> roles =
                [
                    new (UserRoles.User),
                    new (UserRoles.Admin),
                ];
            return roles;
        }
    }
}
