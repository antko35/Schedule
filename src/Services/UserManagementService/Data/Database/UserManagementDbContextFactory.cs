namespace UserManagementService.DataAccess.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public sealed class UserManagementDbContextFactory : IDesignTimeDbContextFactory<UserManagementDbContext>
{
    private const string ConnectionStringEnvironmentVariable = "ConnectionStrings__applicationDb";

    public UserManagementDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Set {ConnectionStringEnvironmentVariable} before running Entity Framework Core tools.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<UserManagementDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            options => options.MigrationsHistoryTable(
                "__EFMigrationsHistory",
                UserManagementDbContext.SchemaName));

        return new UserManagementDbContext(optionsBuilder.Options);
    }
}
