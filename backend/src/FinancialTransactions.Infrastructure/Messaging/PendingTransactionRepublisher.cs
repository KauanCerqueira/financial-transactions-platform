using FinancialTransactions.Application.Abstractions.Messaging;
using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Features.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using RabbitMQ.Client.Exceptions;

namespace FinancialTransactions.Infrastructure.Messaging;

public sealed class PendingTransactionRepublisher(
    IServiceScopeFactory scopeFactory,
    ITransactionQueue queue,
    TimeProvider timeProvider,
    ILogger<PendingTransactionRepublisher> logger) : BackgroundService
{
    private const int BatchSize = 100;
    private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!queue.IsEnabled)
        {
            return;
        }

        using var timer = new PeriodicTimer(RetryInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await RepublishPendingAsync(stoppingToken);
            }
            catch (RabbitMQClientException exception)
            {
                logger.LogWarning(exception, "Fila indisponível; eventos pendentes serão reenviados na próxima tentativa.");
            }
            catch (IOException exception)
            {
                logger.LogWarning(exception, "Falha de transporte; eventos pendentes serão reenviados na próxima tentativa.");
            }
            catch (NpgsqlException exception)
            {
                logger.LogWarning(exception, "Banco indisponível; eventos pendentes serão consultados na próxima tentativa.");
            }
        }
    }

    public async Task RepublishPendingAsync(CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var events = scope.ServiceProvider.GetRequiredService<ITransactionEventRepository>();
        var receivedBefore = timeProvider.GetUtcNow() - RetryInterval;
        var pending = await events.GetPendingBeforeAsync(receivedBefore, BatchSize, cancellationToken);

        foreach (var transactionEvent in pending)
        {
            var command = new ProcessTransactionCommand(
                transactionEvent.EventId,
                transactionEvent.AccountId,
                transactionEvent.Type,
                transactionEvent.Amount.Amount,
                transactionEvent.OccurredAt);

            await queue.PublishAsync(command, cancellationToken);
        }
    }
}
