namespace UserManagementService.PostgreSqlIntegration;

using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Hangfire;
using Hangfire.Storage;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Testcontainers.PostgreSql;
using UserManagementService.Application.Jobs;
using UserManagementService.Application.UseCases.Queries.User;
using UserManagementService.DataAccess.Database;
using UserManagementService.Domain.Abstractions.IRabbitMq;
using Xunit;

public class ApiTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer database = new PostgreSqlBuilder().WithImage("postgres:17.6-alpine").Build();
    private readonly Mock<IUserEventPublisher> publisher = new();
    private WebApplicationFactory<Program> factory = null!;
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        await database.StartAsync();
        Start();
    }

    private void Start()
    {
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:applicationDb", database.GetConnectionString());
            builder.UseSetting("ConnectionStrings:rabbitmq", "amqp://guest:guest@localhost:5672");
            builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
            builder.UseSetting("Keycloak:PublicUrl", "http://localhost:8080");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IHostedService>();
                services.RemoveAll<IUserEventPublisher>();
                services.AddSingleton(publisher.Object);
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, AuthenticationHandler>("Test", _ => { });
            });
        });
        client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        client.DefaultRequestHeaders.Add("X-Test-User", "test");
    }

    public async Task DisposeAsync()
    {
        client.Dispose();
        await factory.DisposeAsync();
        await database.DisposeAsync();
    }

    private async Task<JsonElement> Post(string route, object body)
    {
        var response = await client.PostAsJsonAsync(route, body);
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private async Task<string> User(string name = "Anna") => (await Post("/User", new
    {
        firstName = name, lastName = "Test", patronymic = "P", gender = "Female", dateOfBirth = "1990-02-28",
    })).GetProperty("id").GetString()!;

    private async Task<string> Department() => (await Post("/Department", new
    {
        deartmentName = "D", clinicId = (string?)null,
    })).GetProperty("id").GetString()!;

    private Task<JsonElement> Assign(string userId, string departmentId) => Post("/Department/addUserToDepartment", new
    {
        userId, departmentId, role = "departmentHead", status = "active", email = "head@example.test", phoneNumber = "",
    });

    [Fact]
    public async Task HttpRoundTripUpdatesDeletionEventsAndRestart()
    {
        var userId = await User();
        var otherId = await User("Boris");
        var departmentId = await Department();
        var otherDepartment = await Department();
        Assert.Matches("^[0-9a-f]{24}$", userId);
        var membership = await Assign(userId, departmentId);
        Assert.Equal(JsonValueKind.Null, membership.GetProperty("phoneNumber").ValueKind);
        await Assign(userId, otherDepartment);
        await Assign(otherId, departmentId);
        var edit = await client.PutAsJsonAsync("/Department/editUserInDepartment", new
        {
            userId, departmentId, role = "departmentHead", status = "edited", email = "updated@example.test", phoneNumber = "",
        });
        Assert.True(edit.IsSuccessStatusCode, await edit.Content.ReadAsStringAsync());
        var update = await client.PutAsJsonAsync("/User", new
        {
            userId, firstName = "Updated", lastName = "Test", patronymic = "P", gender = "Female", dateOfBirth = "1995-05-15",
        });
        Assert.True(update.IsSuccessStatusCode, await update.Content.ReadAsStringAsync());
        using (var scope = factory.Services.CreateScope())
        {
            var emails = await scope.ServiceProvider.GetRequiredService<IMediator>()
                .Send(new GetDepartmentHeadsEmailsQuery(new List<string> { departmentId }));
            Assert.Contains("updated@example.test", emails);
            var db = scope.ServiceProvider.GetRequiredService<UserManagementDbContext>();
            Assert.Equal("Updated", (await db.Users.SingleAsync(user => user.Id == userId)).FirstName);
            Assert.Equal(new DateOnly(1995, 5, 15), (await db.Users.SingleAsync(user => user.Id == userId)).DateOfBirth);
        }
        client.Dispose();
        await factory.DisposeAsync();
        Start();
        Assert.Equal(2, (await client.GetFromJsonAsync<JsonElement>("/User")).GetArrayLength());
        Assert.True((await client.DeleteAsync($"/User/{userId}")).IsSuccessStatusCode);
        publisher.Verify(events => events.PublishUserDeleted(userId, departmentId), Times.Once);
        publisher.Verify(events => events.PublishUserDeleted(userId, otherDepartment), Times.Once);
        Assert.True((await client.DeleteAsync($"/Department/delete/{departmentId}")).IsSuccessStatusCode);
        publisher.Verify(events => events.PublishUserDeleted(otherId, departmentId), Times.Once);
        using var verifyScope = factory.Services.CreateScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<UserManagementDbContext>();
        Assert.Single(await verify.Users.ToListAsync());
        Assert.Empty(await verify.UserJobs.ToListAsync());
        Assert.Single(await verify.Departments.ToListAsync());
    }

    [Fact]
    public async Task ValidationAuthenticationDuplicatesAndClinicCompatibility()
    {
        client.DefaultRequestHeaders.Remove("X-Test-User");
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/User")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/Department")).StatusCode);
        client.DefaultRequestHeaders.Add("X-Test-User", "test");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/User/invalid")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/User/aaaaaaaaaaaaaaaaaaaaaaaa")).StatusCode);
        var clinicId = "bbbbbbbbbbbbbbbbbbbbbbbb";
        var department = await Post("/Department", new { deartmentName = "Compatible", clinicId });
        Assert.Equal(clinicId, department.GetProperty("clinicId").GetString());
        var duplicate = await client.PostAsJsonAsync("/Department", new { deartmentName = "Compatible", clinicId });
        Assert.Equal(HttpStatusCode.NotFound, duplicate.StatusCode);
        Assert.Equal("This department already exist", (await duplicate.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error").GetString());
        var concurrent = await Task.WhenAll(
            client.PostAsJsonAsync("/Department", new { deartmentName = "Concurrent", clinicId }),
            client.PostAsJsonAsync("/Department", new { deartmentName = "Concurrent", clinicId }));
        Assert.Single(concurrent, response => response.StatusCode == HttpStatusCode.OK);
        // The application pre-check returns 404; a race reaching the DB constraint returns 500.
        Assert.Single(concurrent, response => response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.InternalServerError);
        var user = await User();
        var departmentId = department.GetProperty("id").GetString()!;
        await Assign(user, departmentId);
        var repeated = await client.PostAsJsonAsync("/Department/addUserToDepartment", new { userId = user, departmentId, role = "doctor", status = "active" });
        Assert.Equal(HttpStatusCode.NotFound, repeated.StatusCode);
        var available = await client.GetFromJsonAsync<JsonElement>($"/User/availableForDepartment/{departmentId}");
        Assert.Empty(available.EnumerateArray());
        var unknown = await client.PostAsJsonAsync("/Department/addUserToDepartment", new { userId = "cccccccccccccccccccccccc", departmentId, role = "doctor", status = "active" });
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.True((await client.DeleteAsync($"/Department/deleteFromDepartment/{user}/{departmentId}")).IsSuccessStatusCode);
        publisher.Verify(events => events.PublishUserDeleted(user, departmentId), Times.Once);
    }

    [Fact]
    public async Task UppercaseHexIdsRetainCanonicalReferencesAndEvents()
    {
        var userId = await User();
        var departmentId = await Department();
        Assert.True((await client.GetAsync($"/User/{userId.ToUpperInvariant()}")).IsSuccessStatusCode);
        var membership = await Assign(userId.ToUpperInvariant(), departmentId.ToUpperInvariant());
        Assert.Equal(userId, membership.GetProperty("userId").GetString());
        Assert.Equal(departmentId, membership.GetProperty("departmentId").GetString());
        Assert.True((await client.DeleteAsync($"/Department/deleteFromDepartment/{userId.ToUpperInvariant()}/{departmentId.ToUpperInvariant()}")).IsSuccessStatusCode);
        publisher.Verify(events => events.PublishUserCreated(userId, departmentId), Times.Once);
        publisher.Verify(events => events.PublishUserDeleted(userId, departmentId), Times.Once);
        Assert.True((await client.DeleteAsync($"/User/{userId.ToUpperInvariant()}")).IsSuccessStatusCode);
    }

    [Fact]
    public async Task BrokerFailureAfterCommitReturnsErrorButRetainsMembership()
    {
        var userId = await User();
        var departmentId = await Department();
        publisher.Setup(events => events.PublishUserCreated(userId, departmentId)).ThrowsAsync(new IOException("broker unavailable"));
        var result = await client.PostAsJsonAsync("/Department/addUserToDepartment", new { userId, departmentId, role = "doctor", status = "active" });
        Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UserManagementDbContext>();
        Assert.Single(await db.UserJobs.Where(job => job.UserId == userId && job.DepartmentId == departmentId).ToListAsync());
    }

    [Fact]
    public async Task DailyJobRunsInScopeAndSurvivesRestartWithoutDuplicateDefinition()
    {
        var userId = await User();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<UserManagementDbContext>();
            await db.Database.ExecuteSqlRawAsync("UPDATE user_management.users SET \"Age\" = 0");
            await ActivatorUtilities.CreateInstance<UpdateUserAgesJob>(scope.ServiceProvider).ExecuteAsync(default);
        }
        var storage = factory.Services.GetRequiredService<JobStorage>();
        using (var connection = storage.GetConnection())
        {
            var job = Assert.Single(connection.GetRecurringJobs());
            Assert.Equal("update-ages-daily", job.Id);
            Assert.Equal("0 0 * * *", job.Cron);
            Assert.Equal("UTC", job.TimeZoneId);
        }
        var scheduleStorage = new Hangfire.PostgreSql.PostgreSqlStorage(
            database.GetConnectionString(),
            new Hangfire.PostgreSql.PostgreSqlStorageOptions { SchemaName = "schedule_hangfire" });
        {
            using var scheduleConnection = scheduleStorage.GetConnection();
            Assert.Empty(scheduleConnection.GetRecurringJobs());
            new RecurringJobManager(scheduleStorage).AddOrUpdate("schedule-only",
                Hangfire.Common.Job.FromExpression(() => Console.WriteLine("schedule")), "0 0 * * *");
            using var managementConnection = storage.GetConnection();
            Assert.DoesNotContain(managementConnection.GetRecurringJobs(), job => job.Id == "schedule-only");
        }
        client.Dispose();
        await factory.DisposeAsync();
        Start();
        using var restarted = factory.Services.GetRequiredService<JobStorage>().GetConnection();
        Assert.Single(restarted.GetRecurringJobs());
        using var verifyScope = factory.Services.CreateScope();
        Assert.True((await verifyScope.ServiceProvider.GetRequiredService<UserManagementDbContext>().Users.SingleAsync(user => user.Id == userId)).Age > 0);
    }

    public class AuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : Microsoft.AspNetCore.Authentication.AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync() => Task.FromResult(
            Request.Headers.ContainsKey("X-Test-User")
                ? AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "Test")), "Test"))
                : AuthenticateResult.NoResult());
    }
}
