using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using UserManagementService.Application.UseCases.Queries.User;

namespace Infrastructure.RabbitMq;

public class UserEmailsRpcServer : BackgroundService
{
    private readonly ILogger<UserEmailsRpcServer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnection _connection; // Внедряется через Aspire
    
    private const string RequestQueue = "get_department_heads_emails";

    public UserEmailsRpcServer(
        ILogger<UserEmailsRpcServer> logger, 
        IServiceScopeFactory scopeFactory,
        IConnection connection) // Получаем уже настроенное соединение
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _connection = connection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Создаем канал
                using var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                // Объявляем очередь для запросов
                await channel.QueueDeclareAsync(
                    queue: RequestQueue, 
                    durable: false, 
                    exclusive: false, 
                    autoDelete: false, 
                    cancellationToken: stoppingToken);

                // Ограничиваем количество одновременных запросов (QoS)
                await channel.BasicQosAsync(0, 1, false, stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);
                
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var departmentsIds = JsonSerializer.Deserialize<List<string>>(body) ?? new List<string>();
                        
                        _logger.LogInformation("RPC Request received for departments: {Departments}", string.Join(", ", departmentsIds));
                         
                        // Получаем данные через MediatR
                        var emails = await GetDepartmentHeadsEmails(departmentsIds, stoppingToken);
                        
                        var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(emails));

                        // Формируем ответные свойства (CorrelationId обязателен для RPC)
                        var replyProps = new BasicProperties
                        {
                            CorrelationId = ea.BasicProperties.CorrelationId
                        };
                        
                        // Отправляем ответ в очередь, указанную в ReplyTo
                        await channel.BasicPublishAsync(
                            exchange: string.Empty,
                            routingKey: ea.BasicProperties.ReplyTo!,
                            mandatory: true,
                            basicProperties: replyProps,
                            body: responseBytes,
                            cancellationToken: stoppingToken);

                        // Подтверждаем обработку сообщения
                        await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing RPC request");
                        // В случае ошибки можно отправить пустой список или Nack
                        await channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                    }
                };

                await channel.BasicConsumeAsync(queue: RequestQueue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

                _logger.LogInformation("RPC Server started. Waiting for requests on queue: {Queue}", RequestQueue);

                // Ожидаем завершения работы
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (BrokerUnreachableException)
            {
                _logger.LogWarning("RabbitMQ is not reachable. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unexpected error in RPC Server");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task<List<string>> GetDepartmentHeadsEmails(List<string> departmentIds, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var query = new GetDepartmentHeadsEmailsQuery(departmentIds);
        var response = await mediator.Send(query, ct);

        _logger.LogInformation("Found {Count} emails", response.Count);
        
        return response;
    }
}