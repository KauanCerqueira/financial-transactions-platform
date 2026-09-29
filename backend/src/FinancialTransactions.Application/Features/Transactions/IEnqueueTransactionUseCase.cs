namespace FinancialTransactions.Application.Features.Transactions;

public interface IEnqueueTransactionUseCase
{
    Task<EnqueuedTransaction> EnqueueAsync(
        ProcessTransactionCommand command,
        CancellationToken cancellationToken = default);
}
