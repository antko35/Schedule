namespace UserService.Infrastructure.Extensions
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using UserService.Domain.Models;
    using Microsoft.AspNetCore.Identity;
    using UserService.Infrastructure.Seeders;

    public static class InfrastractionExtension
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("postgresConnection");
            services.AddEntityFrameworkNpgsql()
                .AddDbContext<UserDbContext>(options => options.UseNpgsql(connectionString));

            services.AddScoped<IUserSeeder, UserSeeder>();
            //services.AddIdentityCore<User>()
            //    .AddEntityFrameworkStores<UserDbContext>()
            //    .AddApiEndpoints();

            //services.AddIdentityApiEndpoints<User>()
            //    .AddEntityFrameworkStores<UserDbContext>();
        }
    }
}
