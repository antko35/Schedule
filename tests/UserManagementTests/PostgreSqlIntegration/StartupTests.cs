namespace UserManagementService.PostgreSqlIntegration;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserManagementService.DataAccess.Extensions;
using Xunit;

public class StartupTests
{
    [Fact]
    public void MissingDatabaseConnectionFailsClearly()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => new ServiceCollection()
            .ConfigureDb(new ConfigurationBuilder().Build()));
        Assert.Contains("applicationDb", exception.Message);
    }

    [Fact]
    public async Task FailedStartupMigrationDoesNotStartServingRequests()
    {
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:applicationDb", "Host=127.0.0.1;Port=1;Database=missing;Username=postgres;Password=postgres;Timeout=1");
            builder.UseSetting("ConnectionStrings:rabbitmq", "amqp://guest:guest@localhost:5672");
            builder.UseSetting("Keycloak:PublicUrl", "http://localhost:8080");
            builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
        });
        Assert.ThrowsAny<Exception>(() => factory.CreateClient());
    }
}
