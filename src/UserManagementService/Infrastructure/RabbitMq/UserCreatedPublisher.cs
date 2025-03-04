using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using UserManagementService.Domain.Abstractions.IRabbitMq;

public class UserCreatedPublisher: IUserCreatedPublisher
{
    private const string ExchangeName = "user_events";

    public async Task PublishUserCreated(string userId, string departmentId)
    {
        var factory = new ConnectionFactory { HostName = "rabbitmq" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        
        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true);
        
        var message = new { UserId = userId, DepartmentId = departmentId };
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        await channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: "user_created",
            body: body);
    }
}