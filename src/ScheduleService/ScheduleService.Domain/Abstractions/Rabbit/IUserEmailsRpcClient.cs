namespace ScheduleService.Domain.Abstractions.Rabbit;

public interface IUserEmailsRpcClient
{
    Task StartAsync();

    Task<string> GetDepartmentHeadsEmailsAsync(List<string> ids,
        CancellationToken cancellationToken = default);

    ValueTask DisposeAsync();
}