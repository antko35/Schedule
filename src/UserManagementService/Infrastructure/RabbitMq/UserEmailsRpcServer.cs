using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UserManagementService.Application.UseCases.Queries.User;

namespace Infrastructure.RabbitMq;

public class UserEmailsRpcServer : IHostedService
{
    private readonly ILogger<UserEmailsRpcServer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private AsyncEventingBasicConsumer? _consumer;
    private readonly ConnectionFactory _factory;
    
    private readonly IServiceScopeFactory scopeFactory;
    
    private const string RequestQueue = "get_department_heads_emails";

    public UserEmailsRpcServer(ILogger<UserEmailsRpcServer> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _factory = new ConnectionFactory { HostName = "rabbitmq" };
        this.scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {

        _connection = await _factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
        
        await _channel.QueueDeclareAsync(queue: RequestQueue, durable: false, exclusive: false, autoDelete: false);

        _consumer = new AsyncEventingBasicConsumer(_channel);
        _consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
           
            var departmentsIds = JsonSerializer.Deserialize<List<string>>(body) ?? new List<string>();
             _logger.LogInformation($"{departmentsIds}");
            var emails = await GetDepartmentHeadsEmails(departmentsIds);
            
            var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(emails));

            IReadOnlyBasicProperties props = ea.BasicProperties;
            var replyProps = new BasicProperties
            {
                CorrelationId = props.CorrelationId
            };
            
            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: ea.BasicProperties.ReplyTo!,
                mandatory: true,
                basicProperties: replyProps,
                body: responseBytes);

            await _channel.BasicAckAsync(ea.DeliveryTag, false);
        };

        await _channel.BasicConsumeAsync(queue: RequestQueue, autoAck: false, consumer: _consumer);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {

        if (_channel is not null)
        {
            await _channel.CloseAsync();
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync();
        }
    }

    private async Task<List<string>> GetDepartmentHeadsEmails(List<string> departmentId)
    {
        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var query = new GetDepartmentHeadsEmailsQuery(departmentId);
        
        var responce = await mediator.Send(query, CancellationToken.None);

        _logger.LogInformation($"{responce.ToArray()}");
        
        return responce;
    }
}