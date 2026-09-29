namespace FinancialTransactions.Application.Features.Transactions;

public interface IGetTransactionEventUseCase
{
    Task<EnqueuedTransaction?> GetAsync(Guid eventId, CancellationToken cancellationToken = default);
}
