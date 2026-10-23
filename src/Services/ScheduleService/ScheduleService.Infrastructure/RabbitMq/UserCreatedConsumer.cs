using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;

public class UserCreatedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnection _connection;
    
    private const string ExchangeName = "user_events";
    private const string QueueName = "user_created_queue";
    private const string RoutingKey = "user_created";

    public UserCreatedConsumer(IServiceScopeFactory scopeFactory, IConnection connection)
    {
        _scopeFactory = scopeFactory;
        _connection = connection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true, cancellationToken: stoppingToken);
                await channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
                await channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey, cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);
                
                consumer.ReceivedAsync += async (sender, eventArgs) =>
                {
                    try
                    {
                        var body = eventArgs.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message);

                        if (userCreatedEvent != null)
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                            var command = new CreateScheduleRulesCommand(userCreatedEvent.UserId, userCreatedEvent.DepartmentId);
                            await mediator.Send(command, stoppingToken);
                        }

                        await channel.BasicAckAsync(eventArgs.DeliveryTag, false, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing message: {ex.Message}");
                    }
                };

                await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, cancellationToken: stoppingToken);

                Console.WriteLine("UserCreatedConsumer: Started and waiting for messages...");

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (BrokerUnreachableException)
            {
                Console.WriteLine("RabbitMQ is not reachable. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in Consumer: {ex.Message}. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
public class UserCreatedEvent
{
    public string UserId { get; set; }
    
    public string DepartmentId { get; set; }
}