using FinancialTransactions.Application.Features.Transactions;

namespace FinancialTransactions.Application.Abstractions.Messaging;

public interface ITransactionQueue
{
    bool IsEnabled { get; }

    Task PublishAsync(ProcessTransactionCommand command, CancellationToken cancellationToken = default);
}
