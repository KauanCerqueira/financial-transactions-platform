namespace FinancialTransactions.Application.Features.Transactions;

public interface IProcessTransactionUseCase
{
    Task<ProcessTransactionResult> ProcessAsync(
        ProcessTransactionCommand command,
        CancellationToken cancellationToken = default);
}
