using System.Text.Json;
using ScheduleService.Domain.Abstractions.Rabbit;

namespace ScheduleService.Application.Services;

public class UserEmailRpcService
{
    private readonly IUserEmailsRpcClient userEmailsRpcClient;

    public UserEmailRpcService(IUserEmailsRpcClient userEmailsRpcClient)
    {
        this.userEmailsRpcClient = userEmailsRpcClient;
    }

    public async Task<List<string>> InvokeAsync(List<string> ids)
    {
        await userEmailsRpcClient.StartAsync();

        var response = await userEmailsRpcClient.GetDepartmentHeadsEmailsAsync(ids);

        var emails = JsonSerializer.Deserialize<List<string>>(response);

        await userEmailsRpcClient.DisposeAsync();

        return emails;
    }
}