using FinancialTransactions.Domain.Entities;

namespace FinancialTransactions.Application.Abstractions.Persistence;

public interface ITransactionEventRepository
{
    Task AddAsync(TransactionEvent transactionEvent, CancellationToken cancellationToken = default);

    Task<TransactionEvent?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TransactionEvent>> GetPendingBeforeAsync(
        DateTimeOffset receivedBefore,
        int limit,
        CancellationToken cancellationToken = default);
}
