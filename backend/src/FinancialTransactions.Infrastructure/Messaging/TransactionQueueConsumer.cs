using System.Text.Json;
using System.Text.Json.Serialization;
using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Exceptions;
using FinancialTransactions.Application.Features.Transactions;
using FinancialTransactions.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FinancialTransactions.Infrastructure.Messaging;

public sealed class TransactionQueueConsumer(
    RabbitMqOptions options,
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<TransactionQueueConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper) }
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.IsConfigured)
        {
            logger.LogWarning("RabbitMQ não configurado; consumidor de transações desligado.");
            return;
        }

        var factory = new ConnectionFactory { Uri = new Uri(options.Uri) };

        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, args) => HandleAsync(channel, args, stoppingToken);

        await channel.BasicConsumeAsync(options.QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        logger.LogInformation("Consumindo a fila {QueueName}.", options.QueueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleAsync(IChannel channel, BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        try
        {
            var command = JsonSerializer.Deserialize<ProcessTransactionCommand>(args.Body.Span, SerializerOptions);

            if (command is null)
            {
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, cancellationToken);
                return;
            }

            await ProcessAsync(command, cancellationToken);

            await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Falha ao processar a mensagem da fila.");
            await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true, cancellationToken);
        }
    }

    private async Task ProcessAsync(ProcessTransactionCommand command, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();

        var transactionEvents = scope.ServiceProvider.GetRequiredService<ITransactionEventRepository>();
        var processTransaction = scope.ServiceProvider.GetRequiredService<IProcessTransactionUseCase>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var transactionEvent = await transactionEvents.GetByEventIdAsync(command.EventId, cancellationToken);

        if (transactionEvent is null)
        {
            logger.LogWarning("Evento {EventId} não encontrado; mensagem descartada.", command.EventId);
            return;
        }

        try
        {
            await processTransaction.ProcessAsync(command, cancellationToken);
            transactionEvent.MarkProcessed(timeProvider.GetUtcNow());
        }
        catch (InsufficientFundsException)
        {
            transactionEvent.MarkRejected("INSUFFICIENT_FUNDS", timeProvider.GetUtcNow());
        }
        catch (AccountNotFoundException)
        {
            transactionEvent.MarkRejected("ACCOUNT_NOT_FOUND", timeProvider.GetUtcNow());
        }
        catch (DomainException)
        {
            transactionEvent.MarkRejected("DOMAIN_ERROR", timeProvider.GetUtcNow());
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
