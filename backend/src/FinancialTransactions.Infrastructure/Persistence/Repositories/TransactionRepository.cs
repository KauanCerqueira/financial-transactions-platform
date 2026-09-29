using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository(AppDbContext dbContext) : ITransactionRepository
{
    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default) =>
        await dbContext.Transactions.AddAsync(transaction, cancellationToken);

    public Task<Transaction?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default) =>
        dbContext.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(transaction => transaction.EventId == eventId, cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        dbContext.Transactions.CountAsync(cancellationToken);

    public async Task<IReadOnlyList<Transaction>> GetPageAsync(
        Guid accountId,
        StatementFilter filter,
        int skip,
        int take,
        CancellationToken cancellationToken = default) =>
        await ApplyFilter(accountId, filter)
            .OrderByDescending(transaction => transaction.OccurredAt)
            .ThenByDescending(transaction => transaction.RecordedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> CountByAccountIdAsync(
        Guid accountId,
        StatementFilter filter,
        CancellationToken cancellationToken = default) =>
        ApplyFilter(accountId, filter).CountAsync(cancellationToken);

    private IQueryable<Transaction> ApplyFilter(Guid accountId, StatementFilter filter)
    {
        var query = dbContext.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.AccountId == accountId);

        if (filter.Type is not null)
        {
            query = query.Where(transaction => transaction.Type == filter.Type);
        }

        if (filter.From is not null)
        {
            query = query.Where(transaction => transaction.OccurredAt >= filter.From);
        }

        if (filter.To is not null)
        {
            query = query.Where(transaction => transaction.OccurredAt <= filter.To);
        }

        return query;
    }
}
