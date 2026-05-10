namespace UserManagementService.DataAccess.Extensions
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using UserManagementService.DataAccess.Database;

    public static class DbConfiguration
    {
        public static IServiceCollection ConfigureDb(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<DbOptions>(
                configuration.GetSection("DatabaseSettings"));

            services.AddSingleton<BaseDbOptions>(sp =>
                sp.GetRequiredService<IOptions<DbOptions>>().Value);

            services.AddScoped<DbContext>();

            return services;
        }
    }
}
