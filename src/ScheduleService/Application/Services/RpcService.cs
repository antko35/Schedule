using System.Text.Json;
using ScheduleService.Domain.Abstractions.Rabbit;

namespace ScheduleService.Application.Services;

public class RpcService
{
    private readonly IUserEmailsRpcClient userEmailsRpcClient;

    public RpcService(IUserEmailsRpcClient userEmailsRpcClient)
    {
        this.userEmailsRpcClient = userEmailsRpcClient;
    }

    public async Task<List<string>> InvokeAsync(List<string> ids)
    {
        await userEmailsRpcClient.StartAsync();

        var response = await userEmailsRpcClient.GetDepartmentHeadsEmailsAsync(ids);

        Console.Out.WriteLine(response);
        var emails = JsonSerializer.Deserialize<List<string>>(response);

        await userEmailsRpcClient.DisposeAsync();

        return emails;
    }
}