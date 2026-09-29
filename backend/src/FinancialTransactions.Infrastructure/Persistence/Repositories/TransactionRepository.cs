using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository(AppDbContext dbContext) : ITransactionRepository
{
    public Task<bool> ExistsByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default) =>
        dbContext.Transactions.AnyAsync(transaction => transaction.EventId == eventId, cancellationToken);

    public Task<Transaction?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default) =>
        dbContext.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(transaction => transaction.EventId == eventId, cancellationToken);

    public async Task<IReadOnlyList<Transaction>> GetPageAsync(
        Guid accountId,
        int skip,
        int take,
        CancellationToken cancellationToken = default) =>
        await dbContext.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.AccountId == accountId)
            .OrderByDescending(transaction => transaction.OccurredAt)
            .ThenByDescending(transaction => transaction.RecordedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> CountByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default) =>
        dbContext.Transactions.CountAsync(transaction => transaction.AccountId == accountId, cancellationToken);
}
