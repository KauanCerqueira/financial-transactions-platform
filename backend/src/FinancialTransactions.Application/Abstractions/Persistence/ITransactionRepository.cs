using FinancialTransactions.Domain.Entities;

namespace FinancialTransactions.Application.Abstractions.Persistence;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task<Transaction?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transaction>> GetPageAsync(
        Guid accountId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
}
