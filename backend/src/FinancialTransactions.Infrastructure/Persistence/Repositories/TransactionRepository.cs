using FinancialTransactions.Application.Abstractions.Persistence;
using FinancialTransactions.Application.Dtos;
using FinancialTransactions.Domain.Entities;
using FinancialTransactions.Domain.Enums;
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

    public async Task<StatementSummaryDto> SummarizeAsync(
        Guid accountId,
        StatementFilter filter,
        CancellationToken cancellationToken = default)
    {
        // Agregação sobre a coluna numérica: o EF não traduz SUM sobre o value object Money.
        var typeName = filter.Type?.ToString();

        var rows = await dbContext.Database
            .SqlQuery<TransactionTypeSummary>($"""
                SELECT type AS "Type", COUNT(*)::int AS "Count", SUM(amount) AS "Total"
                FROM transactions
                WHERE account_id = {accountId}
                  AND ({typeName == null} OR type = {typeName})
                  AND ({filter.From == null} OR occurred_at >= {filter.From})
                  AND ({filter.To == null} OR occurred_at <= {filter.To})
                GROUP BY type
                """)
            .ToListAsync(cancellationToken);

        var credits = rows.SingleOrDefault(row => row.Type == TransactionType.Credit.ToString());
        var debits = rows.SingleOrDefault(row => row.Type == TransactionType.Debit.ToString());

        return new StatementSummaryDto(
            credits?.Count ?? 0,
            credits?.Total ?? 0m,
            debits?.Count ?? 0,
            debits?.Total ?? 0m);
    }

    private sealed class TransactionTypeSummary
    {
        public string Type { get; set; } = string.Empty;

        public int Count { get; set; }

        public decimal Total { get; set; }
    }

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
