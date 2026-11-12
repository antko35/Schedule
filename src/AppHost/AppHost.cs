using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions
{
    Args = args,
    DisableDashboard = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("E2E_RUN_ID")),
});
var e2eRunId = builder.Configuration["E2E_RUN_ID"];
var isE2E = !string.IsNullOrWhiteSpace(e2eRunId);
int TestPort(string key) => int.Parse(builder.Configuration[key]
    ?? throw new InvalidOperationException($"{key} is required in E2E mode."));

var rabbit = builder.AddRabbitMQ("rabbitmq");

var postgres = builder.AddPostgres("application-postgres").WithImageTag("17.6-alpine");
if (!isE2E) postgres.WithDataVolume();
var scheduleDb = postgres.AddDatabase("applicationDb", "application_db");

var keycloak = builder.AddKeycloak("keycloak", port: isE2E ? TestPort("E2E_KEYCLOAK_PORT") : 8080)
    .WithRealmImport(isE2E ? "keycloak-e2e" : "keycloak")
    .WithExternalHttpEndpoints();
if (!isE2E) keycloak.WithDataVolume();
if (isE2E)
{
    postgres.WithContainerName($"schedule-e2e-{e2eRunId}-postgres");
    rabbit.WithContainerName($"schedule-e2e-{e2eRunId}-rabbitmq");
    keycloak.WithContainerName($"schedule-e2e-{e2eRunId}-keycloak");
}
var keycloakPublicUrl = keycloak.GetEndpoint("http");

var scheduleService = builder.AddProject<Projects.ScheduleService_API>("schedule-service")
    .WithReference(scheduleDb)
    .WithEnvironment("Database__ApplyMigrationsOnStartup", "true")
    .WithReference(keycloak)
    .WithReference(rabbit)
    .WithEnvironment("Keycloak__PublicUrl", keycloakPublicUrl)
    .WaitFor(rabbit)
    .WaitFor(scheduleDb)
    .WaitFor(keycloak);

var userService = builder.AddProject<Projects.UserManagementService_API>("user-management-service")
    .WithReference(scheduleDb)
    .WithEnvironment("Database__ApplyMigrationsOnStartup", "true")
    .WithReference(keycloak)
    .WithReference(rabbit)
    .WithEnvironment("Keycloak__PublicUrl", keycloakPublicUrl)
    .WaitFor(rabbit)
    .WaitFor(scheduleDb)
    .WaitFor(keycloak);

var frontend = builder.AddViteApp("scheduleFe", "../../../scheduleFe")
    .WithReference(userService)
    .WithReference(scheduleService)
    .WithReference(keycloak)
    .WithExternalHttpEndpoints()
    .WaitFor(keycloak);

if (isE2E)
{
    userService.WithEndpoint("http", endpoint => endpoint.Port = TestPort("E2E_USER_PORT"));
    scheduleService.WithEndpoint("http", endpoint => endpoint.Port = TestPort("E2E_SCHEDULE_PORT"));
    frontend.WithEndpoint("http", endpoint =>
    {
        endpoint.Port = TestPort("E2E_FRONTEND_PORT");
        endpoint.TargetPort = TestPort("E2E_FRONTEND_PORT");
        endpoint.IsProxied = false;
    });
    frontend.WithNpm(install: false)
        .WithEnvironment("E2E_RUN_ID", e2eRunId!)
        .WithEnvironment("USER_MANAGEMENT_SERVICE_HTTP", userService.GetEndpoint("http"))
        .WithEnvironment("SCHEDULE_SERVICE_HTTP", scheduleService.GetEndpoint("http"));
}
else
{
    frontend.WithHttpEndpoint(name: "angular-ui", port: 4200);
}

await using var app = builder.Build();
if (isE2E)
{
    await app.StartAsync();
    var logs = app.Services.GetRequiredService<ResourceLoggerService>();
    var stopping = app.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping;
    _ = Task.Run(async () =>
    {
        var watched = new HashSet<string>();
        var notifications = app.Services.GetRequiredService<ResourceNotificationService>();
        try
        {
            await foreach (var update in notifications.WatchAsync(stopping))
            {
                // Executable instances have generated IDs distinct from model names.
                if (!watched.Add(update.ResourceId)) continue;
                var resourceId = update.ResourceId;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await foreach (var batch in logs.WatchAsync(resourceId).WithCancellation(stopping))
                        {
                            foreach (var line in batch) Console.WriteLine($"[{resourceId}] {line.Content}");
                        }
                    }
                    catch (OperationCanceledException) when (stopping.IsCancellationRequested) { }
                });
            }
        }
        catch (OperationCanceledException) when (stopping.IsCancellationRequested) { }
    });
    await app.WaitForShutdownAsync();
}
else
{
    await app.RunAsync();
}
