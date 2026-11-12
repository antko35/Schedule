extern alias ScheduleApi;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

// Separate process preserves each service's actual MediatR runtime version.
await using var factory = new WebApplicationFactory<ScheduleApi::Program>().WithWebHostBuilder(builder =>
{
    var root = new DirectoryInfo(AppContext.BaseDirectory);
    while (root is not null && !File.Exists(Path.Combine(root.FullName, "Schedule.sln"))) root = root.Parent;
    builder.UseContentRoot(Path.Combine(root!.FullName, "src/Services/ScheduleService/ScheduleService.API"));
    builder.UseEnvironment("Testing");
    builder.UseSetting("ConnectionStrings:applicationDb", Environment.GetEnvironmentVariable("MIGRATION_TEST_DATABASE"));
    builder.UseSetting("ConnectionStrings:rabbitmq", Environment.GetEnvironmentVariable("MIGRATION_TEST_RABBIT"));
    builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
    builder.UseSetting("Keycloak:PublicUrl", "http://localhost:8080");
    builder.ConfigureTestServices(services =>
    {
        services.RemoveAll<IHostedService>();
        services.AddHostedService<UserCreatedConsumer>();
        services.AddHostedService<UserDeletedConsumer>();
    });
});
using var client = factory.CreateClient();
Console.WriteLine("SCHEDULE_READY");
while (await Console.In.ReadLineAsync() is { } command && command != "stop")
{
    if (!command.StartsWith("rpc:", StringComparison.Ordinal)) continue;
    using var scope = factory.Services.CreateScope();
    var rpc = scope.ServiceProvider.GetRequiredService<ScheduleService.Domain.Abstractions.Rabbit.IUserEmailsRpcClient>();
    await rpc.StartAsync();
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
    var result = await rpc.GetDepartmentHeadsEmailsAsync(new List<string> { command[4..] }, timeout.Token);
    Console.WriteLine("RPC_RESPONSE:" + result);
    await rpc.DisposeAsync();
}
