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

    public async Task<List<string>> InvokeAsync(
        List<string> ids,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await userEmailsRpcClient.StartAsync();

            var response = await userEmailsRpcClient.GetDepartmentHeadsEmailsAsync(ids, cancellationToken);

            return JsonSerializer.Deserialize<List<string>>(response) ?? new List<string>();
        }
        finally
        {
            await userEmailsRpcClient.DisposeAsync();
        }
    }
}
