using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ScheduleService.Domain.Abstractions.Rabbit;

public class UserEmailsRpcClient: IAsyncDisposable, IUserEmailsRpcClient
{
    private const string RequestQueue = "get_department_heads_emails";
    private readonly ConnectionFactory factory;

    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper
        = new();
    
    private IConnection? connection;
    private IChannel? channel;
    private string? replyQueueName;
    
    public UserEmailsRpcClient()
    {
        factory = new ConnectionFactory { HostName = "rabbitmq" };
    }

    public async Task StartAsync()
    {
        connection = await factory.CreateConnectionAsync();
        channel = await connection.CreateChannelAsync();

        // declare a server-named queue
        QueueDeclareOk queueDeclareResult = await channel.QueueDeclareAsync();
        replyQueueName = queueDeclareResult.QueueName;
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += (model, ea) =>
        {
            string? correlationId = ea.BasicProperties.CorrelationId;

            if (false == string.IsNullOrEmpty(correlationId))
            {
                if (callbackMapper.TryRemove(correlationId, out var tcs))
                {
                    var body = ea.Body.ToArray();
                    var response = Encoding.UTF8.GetString(body);
                    tcs.TrySetResult(response);
                }
            }

            return Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(replyQueueName, true, consumer);
    }
    
    public async Task<string> GetDepartmentHeadsEmailsAsync(List<string> ids,
        CancellationToken cancellationToken = default)
    {
        if (channel is null)
        {
            throw new InvalidOperationException();
        }

        string correlationId = Guid.NewGuid().ToString();
        var props = new BasicProperties
        {
            CorrelationId = correlationId,
            ReplyTo = replyQueueName,
        };

        var tcs = new TaskCompletionSource<string>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        callbackMapper.TryAdd(correlationId, tcs);

        string json = JsonSerializer.Serialize(ids);

        var message = Encoding.UTF8.GetBytes(json);
        
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: RequestQueue,
            mandatory: true, basicProperties: props, body: message);

        using CancellationTokenRegistration ctr =
            cancellationToken.Register(() =>
            {
                callbackMapper.TryRemove(correlationId, out _);
                tcs.SetCanceled();
            });

        return await tcs.Task;
    }

    public async ValueTask DisposeAsync()
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
}