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
    private readonly ILogger<UserEmailsRpcServer> logger;
    private IConnection? connection;
    private IChannel? channel;
    private AsyncEventingBasicConsumer? consumer;
    private readonly ConnectionFactory factory;
    
    private readonly IServiceScopeFactory scopeFactory;
    
    private const string RequestQueue = "get_department_heads_emails";

    public UserEmailsRpcServer(ILogger<UserEmailsRpcServer> logger, IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        factory = new ConnectionFactory { HostName = "rabbitmq" };
        this.scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {

        connection = await factory.CreateConnectionAsync();
        channel = await connection.CreateChannelAsync();
        
        await channel.QueueDeclareAsync(queue: RequestQueue, durable: false, exclusive: false, autoDelete: false);

        consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
           
            var departmentsIds = JsonSerializer.Deserialize<List<string>>(body) ?? new List<string>();
            
            logger.LogInformation($"{departmentsIds}");
             
            var emails = await GetDepartmentHeadsEmails(departmentsIds);
            
            var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(emails));

            IReadOnlyBasicProperties props = ea.BasicProperties;
            var replyProps = new BasicProperties
            {
                CorrelationId = props.CorrelationId
            };
            
            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: ea.BasicProperties.ReplyTo!,
                mandatory: true,
                basicProperties: replyProps,
                body: responseBytes);

            await channel.BasicAckAsync(ea.DeliveryTag, false);
        };

        await channel.BasicConsumeAsync(queue: RequestQueue, autoAck: false, consumer: consumer);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {

        if (channel is not null)
        {
            await channel.CloseAsync();
        }

        if (connection is not null)
        {
            await connection.CloseAsync();
        }
    }

    private async Task<List<string>> GetDepartmentHeadsEmails(List<string> departmentId)
    {
        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var query = new GetDepartmentHeadsEmailsQuery(departmentId);
        
        var responce = await mediator.Send(query, CancellationToken.None);

        logger.LogInformation($"{responce.ToArray()}");
        
        return responce;
    }
}