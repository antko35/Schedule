namespace ScheduleService.PostgreSqlIntegration;

using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using ScheduleService.DataAccess.Database;
using Testcontainers.PostgreSql;
using Xunit;

// Uses the real HTTP pipeline, validators, handlers, EF migrations and PostgreSQL.
// Authentication is substituted; background jobs/consumers cannot contact external systems.
public sealed class ApiMigrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer database = new PostgreSqlBuilder().WithImage("postgres:17.6-alpine").Build();
    private WebApplicationFactory<Program> factory = null!;
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        await database.StartAsync();
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
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", _ => { });
            });
        });
        client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
    }

    public async Task DisposeAsync()
    {
        client?.Dispose();
        if (factory is not null) await factory.DisposeAsync();
        await database.DisposeAsync();
    }

    [Fact]
    public async Task RulesGenerationManualReplacementAndDeletionRoundTrip()
    {
        var route = "/ScheduleRules/rules/user-api/department-api/2025/2";
        var response = await client.PutAsJsonAsync(route, new
        {
            hoursPerMonth = 160, maxHoursPerDay = 8, startWorkDayTime = "08:00:00", onlyFirstShift = true,
        });
        await EnsureSuccess(response);
        var rule = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("february", rule.GetProperty("monthName").GetString());
        Assert.False(string.IsNullOrEmpty(rule.GetProperty("scheduleId").GetString()));
        var id = rule.GetProperty("id").GetString();
        await EnsureSuccess(await client.PatchAsJsonAsync("/ScheduleRules/setRules", new { scheduleRulesId = id, startWorkDayTime = "00:00:00" }));
        var patched = await client.GetFromJsonAsync<JsonElement>(route);
        Assert.Equal("00:00:00", patched.GetProperty("startWorkDayTime").GetString());
        Assert.True(patched.GetProperty("onlyFirstShift").GetBoolean());
        await EnsureSuccess(await client.PostAsJsonAsync("/Schedule/user-api/generate", new { departmentId = "department-api", year = 2025, month = 2 }));
        var schedule = await client.GetFromJsonAsync<JsonElement>("/Schedule/schedule/user-api/department-api/2025/2");
        Assert.Equal(20, schedule.GetProperty("workDays").GetArrayLength());
        // Regeneration must replace tracked rows without unique-date constraint failures.
        await EnsureSuccess(await client.PostAsJsonAsync("/Schedule/user-api/generate", new { departmentId = "department-api", year = 2025, month = 2 }));
        await EnsureSuccess(await client.PostAsJsonAsync("/Schedule", new
        {
            userId = "user-api", departmentId = "department-api", startTime = "2025-02-03T09:00:00Z", endTime = "2025-02-03T15:00:00Z",
        }));
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();
        Assert.Equal(20, await db.WorkShifts.CountAsync());
        Assert.Equal(15, (await db.WorkShifts.SingleAsync(x => x.WorkDate == new DateOnly(2025, 2, 3))).EndsAtUtc.Hour);
        Assert.Contains("20261024070902_InitialScheduleSchema", await db.Database.GetAppliedMigrationsAsync());
        // Existing export route must still produce a workbook from relational schedules.
        var export = await client.PostAsJsonAsync("/Schedule/export", new
        {
            departmentId = "department-api", departmentName = "Test", year = 2025, month = 2,
            users = new[] { new { userId = "user-api", fullName = "Test User" } },
        });
        await EnsureSuccess(export);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", export.Content.Headers.ContentType!.MediaType);
        var workbook = await export.Content.ReadAsByteArrayAsync();
        Assert.Equal((byte)'P', workbook[0]);
        Assert.Equal((byte)'K', workbook[1]);
        await EnsureSuccess(await client.DeleteAsync("/ScheduleRules?userId=user-api&departmentId=department-api"));
        db.ChangeTracker.Clear();
        Assert.Equal(0, await db.WorkShifts.CountAsync());
        Assert.Equal(0, await db.MonthlySchedules.CountAsync());
        Assert.Equal(0, await db.ScheduleGenerationRules.CountAsync());
    }

    [Fact]
    public async Task ManualEntryCreatesPreviouslyMissingMonthAndRules()
    {
        await EnsureSuccess(await client.PostAsJsonAsync("/Schedule", new
        {
            userId = "new-user", departmentId = "new-department", startTime = "2026-10-05T08:00:00Z", endTime = "2026-10-05T16:00:00Z",
        }));
        var schedule = await client.GetFromJsonAsync<JsonElement>("/Schedule/schedule/new-user/new-department/2026/10");
        Assert.Single(schedule.GetProperty("workDays").EnumerateArray());
        var rules = await client.GetFromJsonAsync<JsonElement>("/ScheduleRules/rules/new-user/new-department/2026/10");
        Assert.Equal(schedule.GetProperty("id").GetString(), rules.GetProperty("scheduleId").GetString());
    }

    [Fact]
    public async Task CalendarBulkDeleteIsAtomicAndKeepsResponseContract()
    {
        await EnsureSuccess(await client.PostAsJsonAsync("/Calendar", new { holiday = "2026-01-01" }));
        var all = await client.GetFromJsonAsync<JsonElement>("/Calendar/holidays");
        Assert.Single(all.EnumerateArray());
        Assert.Equal("2026-01-01", all[0].GetProperty("holidayDate").GetString());
        using var invalid = new HttpRequestMessage(HttpMethod.Delete, "/Calendar/batch")
            { Content = JsonContent.Create(new { holidays = new[] { "2026-01-01", "2026-01-02" } }) };
        Assert.Equal(HttpStatusCode.NotFound, (await client.SendAsync(invalid)).StatusCode);
        Assert.Single((await client.GetFromJsonAsync<JsonElement>("/Calendar/holidays")).EnumerateArray());
        using var valid = new HttpRequestMessage(HttpMethod.Delete, "/Calendar/batch")
            { Content = JsonContent.Create(new { holidays = new[] { "2026-01-01", "2026-01-01" } }) };
        Assert.Equal(HttpStatusCode.NoContent, (await client.SendAsync(valid)).StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<JsonElement>("/Calendar/holidays")).EnumerateArray());
    }

    private static async Task EnsureSuccess(HttpResponseMessage response) =>
        Assert.True(response.IsSuccessStatusCode, $"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
}

public sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync() => Task.FromResult(
        AuthenticateResult.Success(new AuthenticationTicket(
            new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "migration-test")], "Test")), "Test")));
}
