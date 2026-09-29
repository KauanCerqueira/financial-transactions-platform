using System.Text.Json;
using System.Text.Json.Serialization;
using FinancialTransactions.Application.Abstractions.Messaging;
using FinancialTransactions.Application.Features.Transactions;
using RabbitMQ.Client;

namespace FinancialTransactions.Infrastructure.Messaging;

public sealed class RabbitMqTransactionQueue(RabbitMqOptions options) : ITransactionQueue, IAsyncDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper) }
    };

    private readonly SemaphoreSlim gate = new(1, 1);
    private IConnection? connection;
    private IChannel? channel;

    public bool IsEnabled => options.IsConfigured;

    public async Task PublishAsync(ProcessTransactionCommand command, CancellationToken cancellationToken = default)
    {
        var publishChannel = await GetChannelAsync(cancellationToken);
        var payload = JsonSerializer.SerializeToUtf8Bytes(command, SerializerOptions);
        var properties = new BasicProperties { Persistent = true, ContentType = "application/json" };

        await publishChannel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: options.QueueName,
            mandatory: false,
            basicProperties: properties,
            body: payload,
            cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (channel is not null)
        {
            await channel.DisposeAsync();
        }

        if (connection is not null)
        {
            await connection.DisposeAsync();
        }

        gate.Dispose();
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (channel is { IsOpen: true })
        {
            return channel;
        }

        await gate.WaitAsync(cancellationToken);

        try
        {
            if (channel is { IsOpen: true })
            {
                return channel;
            }

            var factory = new ConnectionFactory { Uri = new Uri(options.Uri) };

            connection = await factory.CreateConnectionAsync(cancellationToken);
            channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
            await channel.QueueDeclareAsync(
                options.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            return channel;
        }
        finally
        {
            gate.Release();
        }
    }
}
