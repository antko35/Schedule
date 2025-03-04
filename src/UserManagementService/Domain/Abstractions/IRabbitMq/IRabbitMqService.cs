namespace UserManagementService.Domain.Abstractions.IRabbitMq;

public interface IRabbitMqService
{
    void SendMessage(object obj);

    Task SendMessage<T>(T message);
}