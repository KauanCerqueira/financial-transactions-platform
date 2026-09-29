using FinancialTransactions.Application.Abstractions.Messaging;
using FinancialTransactions.Application.Features.Transactions;

namespace FinancialTransactions.Infrastructure.Messaging;

public sealed class DisabledTransactionQueue : ITransactionQueue
{
    public bool IsEnabled => false;

    public Task PublishAsync(ProcessTransactionCommand command, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("A fila de transações não está configurada.");
}
