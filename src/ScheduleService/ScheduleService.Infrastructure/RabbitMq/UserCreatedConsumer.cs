using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;

public class UserCreatedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private const string ExchangeName = "user_events";
    private const string QueueName = "user_created_queue";
    private readonly ConnectionFactory factory;

    private IConnection? connection;
    private IChannel? channel;

    public UserCreatedConsumer(IServiceScopeFactory scopeFactory)
    {
        this.scopeFactory = scopeFactory;
        factory = new ConnectionFactory { HostName = "rabbitmq" };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        connection = await factory.CreateConnectionAsync();
        channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true);
        await channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false);
        await channel.QueueBindAsync(QueueName, ExchangeName, "user_created");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message);

            if (userCreatedEvent != null)
            {
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var command = new CreateScheduleRulesCommand(userCreatedEvent.UserId, userCreatedEvent.DepartmentId);
                await mediator.Send(command, stoppingToken);
            }

            await channel!.BasicAckAsync(eventArgs.DeliveryTag, false);
            
        };

        await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (channel != null)
            await channel.CloseAsync();
        
        if (connection != null)
            await connection.CloseAsync();

        await base.StopAsync(cancellationToken);
    }
}

public class UserCreatedEvent
{
    public string UserId { get; set; }
    
    public string DepartmentId { get; set; }
}
