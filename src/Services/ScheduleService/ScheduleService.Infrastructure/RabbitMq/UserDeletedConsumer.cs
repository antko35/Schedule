using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using ScheduleService.Application.UseCases.Commands.ScheduleRules;

public class UserDeletedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnection _connection; // Внедряется через Aspire (уже настроено)
    
    private const string ExchangeName = "user_events";
    private const string QueueName = "user_deleted_queue";
    private const string RoutingKey = "user_deleted";

    public UserDeletedConsumer(IServiceScopeFactory scopeFactory, IConnection connection)
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
                // Создаем канал через внедренное соединение
                using var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                // Декларируем инфраструктуру (Exchange, Queue, Binding)
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
                        var userDeletedEvent = JsonSerializer.Deserialize<UserDeletedEvent>(message);

                        if (userDeletedEvent != null)
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                            // Вызываем команду удаления
                            var command = new DeleteRulesAndScheduleCommand(userDeletedEvent.UserId, userDeletedEvent.DepartmentId);
                            await mediator.Send(command, stoppingToken);
                        }

                        // Подтверждаем успешную обработку
                        await channel.BasicAckAsync(eventArgs.DeliveryTag, false, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[UserDeletedConsumer] Error processing message: {ex.Message}");
                        // В случае ошибки можно не подтверждать сообщение (Nack), чтобы оно вернулось в очередь
                        // await channel.BasicNackAsync(eventArgs.DeliveryTag, false, requeue: true, stoppingToken);
                    }
                };

                await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, cancellationToken: stoppingToken);

                Console.WriteLine("[UserDeletedConsumer] Waiting for messages...");

                // Держим сервис запущенным, пока не придет сигнал остановки
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (BrokerUnreachableException)
            {
                Console.WriteLine("[UserDeletedConsumer] RabbitMQ is not reachable. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break; // Нормальный выход при остановке приложения
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserDeletedConsumer] Unexpected error: {ex.Message}. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
    
    // Метод StopAsync теперь можно не переопределять, так как IConnection управляется DI контейнером,
    // а IChannel закрывается через 'using'. Но если оставите - вреда не будет.
}

public class UserDeletedEvent
{
    public string UserId { get; set; } = string.Empty;
    public string DepartmentId { get; set; } = string.Empty;
}