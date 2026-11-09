namespace UserManagementService.DataAccess.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserManagementService.DataAccess.Database;

public static class DbConfiguration
{
    public const string ConnectionStringName = "applicationDb";

    public static IServiceCollection ConfigureDb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not configured.");
        }

        services.AddDbContext<UserManagementDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                    "__EFMigrationsHistory",
                    UserManagementDbContext.SchemaName)));

        services.AddScoped<UserManagementService.Domain.Abstractions.IRepository.IDataChanges>(
            provider => provider.GetRequiredService<UserManagementDbContext>());
        return services;
    }
}
