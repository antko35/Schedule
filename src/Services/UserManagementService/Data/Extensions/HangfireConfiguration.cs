namespace UserManagementService.DataAccess.Extensions;

using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class HangfireConfiguration
{
    private const string HangfireSchemaName = "user_management_hangfire";

    public static IServiceCollection ConfigureHangfire(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DbConfiguration.ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{DbConfiguration.ConnectionStringName}' is not configured.");
        }

        services.AddHangfire((_, hangfireConfiguration) =>
            hangfireConfiguration.UsePostgreSqlStorage(
                options => options.UseNpgsqlConnection(connectionString),
                new PostgreSqlStorageOptions
                {
                    SchemaName = HangfireSchemaName,
                    PrepareSchemaIfNecessary = true,
                }));

        services.AddHangfireServer();

        return services;
    }
}
