namespace UserManagementService.PostgreSqlIntegration;

using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Infrastructure.RabbitMq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using ScheduleService.DataAccess.Database;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;

public class CrossServiceTests
{
    [Fact]
    public async Task BothApisAndRealRabbitMqWorkWithoutMongoAndSurviveRestart()
    {
        await using var database = new PostgreSqlBuilder().WithImage("postgres:17.6-alpine").Build();
        await using var rabbit = new RabbitMqBuilder().WithImage("rabbitmq:4.0-management").Build();
        await Task.WhenAll(database.StartAsync(), rabbit.StartAsync());
        var dbConnection = database.GetConnectionString();
        var rabbitConnection = rabbit.GetConnectionString();

        WebApplicationFactory<Program> Management() => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            Settings(builder);
            builder.ConfigureTestServices(services =>
            {
                Authenticate(services);
                services.RemoveAll<IHostedService>();
                services.AddHostedService<UserEmailsRpcServer>();
            });
        });

        void Settings(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:applicationDb", dbConnection);
            builder.UseSetting("ConnectionStrings:rabbitmq", rabbitConnection);
            builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
            builder.UseSetting("Keycloak:PublicUrl", "http://localhost:8080");
        }

        var management = Management();
        var schedule = await ScheduleHost.Start(dbConnection, rabbitConnection);
        HttpClient managementClient = Client(management);
        try
        {
            await WaitForQueues(management.Services, "user_created_queue", "user_deleted_queue");
            await WaitForQueues(management.Services, "get_department_heads_emails");
            var userId = (await Post("/User", new { firstName = "Test", lastName = "User", patronymic = "P", gender = "Female", dateOfBirth = "1990-01-01" })).GetProperty("id").GetString()!;
            var departmentId = (await Post("/Department", new { deartmentName = "Department" })).GetProperty("id").GetString()!;
            await Post("/Department/addUserToDepartment", new { userId, departmentId, role = "departmentHead", status = "active", email = "head@example.test" });
            await Eventually(async () =>
            {
                await using var db = OpenSchedule(dbConnection);
                return await db.MonthlySchedules.AnyAsync(item => item.UserId == userId && item.DepartmentId == departmentId);
            });
            var emails = JsonSerializer.Deserialize<List<string>>(await schedule.RequestEmails(departmentId));
            Assert.Equal(new[] { "head@example.test" }, emails);
            var edited = await managementClient.PutAsJsonAsync("/Department/editUserInDepartment", new { userId, departmentId, role = "doctor", status = "edited" });
            Assert.True(edited.IsSuccessStatusCode, await edited.Content.ReadAsStringAsync());

            managementClient.Dispose();
            await management.DisposeAsync(); await schedule.DisposeAsync();
            management = Management(); schedule = await ScheduleHost.Start(dbConnection, rabbitConnection);
            managementClient = Client(management);
            await WaitForQueues(management.Services, "user_created_queue", "user_deleted_queue");
            Assert.Single((await managementClient.GetFromJsonAsync<JsonElement>("/User")).EnumerateArray());
            await using (var db = OpenSchedule(dbConnection))
            {
                Assert.Single(await db.MonthlySchedules.ToListAsync());
            }
            var deleted = await managementClient.DeleteAsync($"/Department/deleteFromDepartment/{userId}/{departmentId}");
            Assert.True(deleted.IsSuccessStatusCode, await deleted.Content.ReadAsStringAsync());
            await Eventually(async () =>
            {
                await using var db = OpenSchedule(dbConnection);
                return !await db.MonthlySchedules.AnyAsync() && !await db.ScheduleGenerationRules.AnyAsync();
            });
            Assert.True((await managementClient.DeleteAsync($"/User/{userId}")).IsSuccessStatusCode);
            Assert.True((await managementClient.DeleteAsync($"/Department/delete/{departmentId}")).IsSuccessStatusCode);
        }
        finally
        {
            managementClient.Dispose();
            await management.DisposeAsync(); await schedule.DisposeAsync();
        }

        async Task<JsonElement> Post(string route, object body)
        {
            var response = await managementClient.PostAsJsonAsync(route, body);
            Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
            return await response.Content.ReadFromJsonAsync<JsonElement>();
        }
    }

    private static ScheduleDbContext OpenSchedule(string connection) => new(
        new DbContextOptionsBuilder<ScheduleDbContext>().UseNpgsql(connection,
            options => options.MigrationsHistoryTable("__EFMigrationsHistory", "schedule")).Options);

    private sealed class ScheduleHost(Process process, Task output, Task errors, System.Threading.Channels.Channel<string> responses) : IAsyncDisposable
    {
        public static async Task<ScheduleHost> Start(string database, string rabbit)
        {
            var configuration = new DirectoryInfo(AppContext.BaseDirectory).Parent!.Name;
            var executable = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
                "../../../../ScheduleConsumerHost/bin", configuration, "net10.0/ScheduleConsumerHost.dll"));
            var start = new ProcessStartInfo("dotnet")
            {
                RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
                UseShellExecute = false,
            };
            start.ArgumentList.Add(executable);
            start.Environment["MIGRATION_TEST_DATABASE"] = database;
            start.Environment["MIGRATION_TEST_RABBIT"] = rabbit;
            var process = Process.Start(start)!;
            var responses = System.Threading.Channels.Channel.CreateUnbounded<string>();
            var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var logs = new System.Text.StringBuilder();
            var output = Task.Run(async () =>
            {
                while (await process.StandardOutput.ReadLineAsync() is { } line)
                {
                    logs.AppendLine(line);
                    if (line == "SCHEDULE_READY") ready.TrySetResult();
                    if (line.StartsWith("RPC_RESPONSE:", StringComparison.Ordinal)) responses.Writer.TryWrite(line[13..]);
                }
                ready.TrySetException(new InvalidOperationException(logs.ToString()));
            });
            var errors = process.StandardError.ReadToEndAsync();
            var host = new ScheduleHost(process, output, errors, responses);
            try { await ready.Task.WaitAsync(TimeSpan.FromSeconds(30)); }
            catch (Exception exception) { await host.DisposeAsync(); throw new InvalidOperationException(logs.ToString() + await errors, exception); }
            return host;
        }

        public async Task<string> RequestEmails(string departmentId)
        {
            await process.StandardInput.WriteLineAsync("rpc:" + departmentId);
            return await responses.Reader.ReadAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(20));
        }

        public async ValueTask DisposeAsync()
        {
            if (!process.HasExited)
            {
                await process.StandardInput.WriteLineAsync("stop");
                try { await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10)); }
                catch (TimeoutException) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); }
            }
            await Task.WhenAll(output, errors);
            process.Dispose();
        }
    }

    private static void Authenticate(IServiceCollection services) => services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Test";
        options.DefaultChallengeScheme = "Test";
    }).AddScheme<AuthenticationSchemeOptions, ApiTests.AuthenticationHandler>("Test", _ => { });

    private static HttpClient Client<T>(WebApplicationFactory<T> factory) where T : class
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        client.DefaultRequestHeaders.Add("X-Test-User", "test");
        return client;
    }

    private static async Task WaitForQueues(IServiceProvider services, params string[] queues)
    {
        var connection = services.GetRequiredService<IConnection>();
        foreach (var queue in queues)
        {
            await Eventually(async () =>
            {
                try
                {
                    using var channel = await connection.CreateChannelAsync();
                    await channel.QueueDeclarePassiveAsync(queue);
                    return true;
                }
                catch (RabbitMQ.Client.Exceptions.OperationInterruptedException) { return false; }
            });
        }
    }

    private static async Task Eventually(Func<Task<bool>> predicate)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        while (!await predicate()) await Task.Delay(100, timeout.Token);
    }
}
