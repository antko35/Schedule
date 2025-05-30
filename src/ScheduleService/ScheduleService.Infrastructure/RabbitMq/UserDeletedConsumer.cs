using System.Text;
using System.Text.Json;
using MediatR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;

public class UserDeletedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private const string ExchangeName = "user_events";
    private const string QueueName = "user_deleted_queue";
    private readonly ConnectionFactory factory;

    private IConnection? connection;
    private IChannel? channel;

    public UserDeletedConsumer(IServiceScopeFactory scopeFactory)
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
        await channel.QueueBindAsync(QueueName, ExchangeName, "user_deleted");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var userDeletedEvent = JsonSerializer.Deserialize<UserDeletedEvent>(message);

            if (userDeletedEvent != null)
            {
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var command = new DeleteRulesAndScheduleCommand(userDeletedEvent.UserId, userDeletedEvent.DepartmentId);
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

public class UserDeletedEvent
{
    public string UserId { get; set; }
    
    public string DepartmentId { get; set; }
}
