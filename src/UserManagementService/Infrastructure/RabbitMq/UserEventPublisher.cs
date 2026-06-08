using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using UserManagementService.Domain.Abstractions.IRabbitMq;

namespace Infrastructure.RabbitMq;

public class UserEventPublisher: IUserEventPublisher
{
    private const string ExchangeName = "user_events";
    private readonly IConfiguration _configuration;

    public UserEventPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task PublishUserCreated(string userId, string departmentId)
    {
        var connectionString =
            _configuration.GetConnectionString("rabbitmq");
        
        var factory = new ConnectionFactory
        {
            Uri = new Uri(connectionString)
        };
        
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
    
    public async Task PublishUserDeleted(string userId, string departmentId)
    {
        var connectionString =
            _configuration.GetConnectionString("rabbitmq");
        
        var factory = new ConnectionFactory
        {
            Uri = new Uri(connectionString)
        };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        
        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true);
        
        var message = new { UserId = userId, DepartmentId =  departmentId};
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        await channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: "user_deleted",
            body: body);
    }
}