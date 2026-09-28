namespace FinancialTransactions.Application.Features.Transactions;

public interface IProcessTransactionUseCase
{
    Task<ProcessTransactionResult> ExecuteAsync(
        ProcessTransactionCommand command,
        CancellationToken cancellationToken = default);
}
