using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using UserService.Domain.Models;

namespace UserService.Infrastructure.Extensions
{
    public static class InfrastractionExtension
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("postgresConnection");
            services.AddEntityFrameworkNpgsql()
                .AddDbContext<UserDbContext>(options => options.UseNpgsql(connectionString));

           
            //services.AddIdentityApiEndpoints<User>()
            //    //.AddRoles<IdentityRole>()
            //    .AddEntityFrameworkStores<UserDbContext>();

        }
    }
}
