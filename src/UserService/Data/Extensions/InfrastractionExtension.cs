namespace UserService.Infrastructure.Extensions
{
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using UserService.Infrastructure.EmailSender;
    using UserService.Infrastructure.Seeders;

    public static class InfrastractionExtension
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("postgresConnection");
            services.AddEntityFrameworkNpgsql()
                .AddDbContext<UserDbContext>(options => options.UseNpgsql(connectionString));

            services.AddScoped<IUserSeeder, UserSeeder>();
            services.AddSingleton<IEmailSender, IdentityEmailSender>();

        }
    }
}
