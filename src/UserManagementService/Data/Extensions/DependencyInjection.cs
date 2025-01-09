namespace UserManagementService.Data.Extensions;


//using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UserManagementService.Data.Database;
using UserManagementService.Data.Repository;
using UserManagementService.Domain.Abstractions.IRepository;

public static class DependencyInjection
{
    public static void AddDataLayer(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        //services.Configure<DbSettings>(configuration.GetSection("DatabaseSettings").ToString(), opt => { });
        //services.AddSingleton<IDbSettings>(sp =>
        //    sp.GetRequiredService<IOptions<DbSettings>>().Value);
        //services.AddSingleton<DbContext>();
    }

    public static void AddDependencis(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
    }
}
