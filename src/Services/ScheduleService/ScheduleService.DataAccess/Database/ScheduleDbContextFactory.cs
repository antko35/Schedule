namespace ScheduleService.DataAccess.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public sealed class ScheduleDbContextFactory : IDesignTimeDbContextFactory<ScheduleDbContext>
{
    private const string ConnectionStringEnvironmentVariable = "ConnectionStrings__applicationDb";

    public ScheduleDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Set {ConnectionStringEnvironmentVariable} before running Entity Framework Core tools.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ScheduleDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            options => options.MigrationsHistoryTable(
                "__EFMigrationsHistory",
                ScheduleDbContext.SchemaName));

        return new ScheduleDbContext(optionsBuilder.Options);
    }
}
